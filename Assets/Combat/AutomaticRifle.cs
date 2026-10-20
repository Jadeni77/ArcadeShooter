using System;
using ArcadeShooter.UI;
using ArcadeShooter.Networking;
using ArcadeShooter.Missions;
using ArcadeShooter.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Combat
{
    // Owns ammo, fire timing, reload timing, and hits. Reloads finish on this timer alone;
    // view-model animation such as WeaponViewModel only observes this state.
    [RequireComponent(typeof(PlayerInput), typeof(WeaponAudio))]
    public sealed class AutomaticRifle : MonoBehaviour
    {
        [Header("Weapon tuning")]
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.1f)] private float roundsPerSecond = 8f;
        [SerializeField, Min(1)] private int magazineSize = 30;
        [SerializeField, Min(0)] private int startingReserve = 90;
        [SerializeField, Min(0.01f)] private float reloadDuration = 1.6f;
        [SerializeField, Min(0.1f)] private float range = 100f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Scene references")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private GameObject impactPrefab;
        [SerializeField] private WeaponHud hud;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float flashDuration = 0.055f;

        private InputAction attackAction;
        private InputAction reloadAction;
        private WeaponAudio weaponAudio;
        private int magazine;
        private int reserve;
        private float nextShotAt;
        private float reloadStartedAt;
        private float reloadEndsAt;
        private float flashEndsAt;
        private float nextEmptySoundAt;
        private bool reloading;
        private NetworkRifle networkRifle;
        private bool CombatAllowed => MissionController.Instance == null || MissionController.Instance.IsRunning;

        public int Magazine => magazine;
        public int Reserve => reserve;
        public int MagazineSize => magazineSize;
        public int StartingReserve => startingReserve;
        public float Damage => damage;
        public float RoundsPerSecond => roundsPerSecond;
        public float ReloadDuration => reloadDuration;
        public float Range => range;
        public LayerMask HitMask => hitMask;
        public bool IsReloading => reloading;
        // 0 at reload start to 1 when the magazine refills; 0 when not reloading.
        public float ReloadProgress =>
            reloading ? Mathf.Clamp01((Time.time - reloadStartedAt) / reloadDuration) : 0f;

        // Raised after each shot's gameplay effects have been applied.
        public event Action Fired;
        public event Action ReloadStarted;
        public event Action ReloadFinished;

        private void Awake()
        {
            PlayerInput input = GetComponent<PlayerInput>();
            attackAction = input.actions.FindAction("Attack", true);
            reloadAction = input.actions.FindAction("Reload", true);
            weaponAudio = GetComponent<WeaponAudio>();
            networkRifle = GetComponent<NetworkRifle>();
            if (aimCamera == null)
                Debug.LogError("AutomaticRifle needs an aim camera.", this);
            magazine = magazineSize;
            reserve = startingReserve;
            if (muzzleFlash != null)
                muzzleFlash.SetActive(false);
            RefreshHud();
        }

        private void Update()
        {
            if (!CombatAllowed)
            {
                if (reloading)
                {
                    reloading = false;
                    ReloadFinished?.Invoke();
                    RefreshHud();
                }
                if (muzzleFlash != null) muzzleFlash.SetActive(false);
                return;
            }
            if (reloading && Time.time >= reloadEndsAt &&
                (networkRifle == null || !networkRifle.IsSpawned))
                CompleteReload();

            if (Cursor.lockState == CursorLockMode.Locked && reloadAction.WasPressedThisFrame())
                BeginReload();

            if (Cursor.lockState == CursorLockMode.Locked && attackAction.IsPressed())
                TryFire();

            if (muzzleFlash != null && muzzleFlash.activeSelf && Time.time >= flashEndsAt)
                muzzleFlash.SetActive(false);
        }

        private bool TryFire()
        {
            if (!CombatAllowed || reloading || aimCamera == null || Time.time < nextShotAt)
                return false;
            if (magazine <= 0)
            {
                if (!BeginReload() && Time.time >= nextEmptySoundAt)
                {
                    weaponAudio.PlayEmpty();
                    nextEmptySoundAt = Time.time + 0.25f;
                }
                return false;
            }

            magazine--;
            nextShotAt = Time.time + 1f / roundsPerSecond;
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                flashEndsAt = Time.time + flashDuration;
            }
            weaponAudio.PlayGunfire();

            Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            if (networkRifle != null)
                networkRifle.RequestShot(ray.direction);
            else if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                PlayConfirmedImpact(hit.point, hit.normal, ImpactEffect.Classify(hit.collider));
                if (impactPrefab == null) weaponAudio.PlayImpact();
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.ApplyDamage(damage, hit.point))
                    hud?.ConfirmHit(damageable is EnemyHealth enemy && enemy.IsDead);
            }
            if (magazine == 0)
                BeginReload();
            RefreshHud();
            Fired?.Invoke();
            return true;
        }

        private bool BeginReload()
        {
            if (!CombatAllowed || reloading || magazine >= magazineSize || reserve <= 0)
                return false;
            reloading = true;
            reloadStartedAt = Time.time;
            reloadEndsAt = reloadStartedAt + reloadDuration;
            ReloadStarted?.Invoke();
            if (networkRifle != null) networkRifle.RequestReload();
            RefreshHud();
            return true;
        }

        public void SyncAmmo(int authoritativeMagazine, int authoritativeReserve, bool authoritativeReloading)
        {
            magazine = authoritativeMagazine;
            reserve = authoritativeReserve;
            if (authoritativeReloading && !reloading)
            {
                reloading = true;
                reloadStartedAt = Time.time;
                reloadEndsAt = Time.time + reloadDuration;
                ReloadStarted?.Invoke();
            }
            else if (!authoritativeReloading && reloading)
            {
                reloading = false;
                ReloadFinished?.Invoke();
            }
            RefreshHud();
        }

        public void PlayRemoteShot()
        {
            weaponAudio.PlayGunfire();
        }

        public void PlayConfirmedImpact(Vector3 point, Vector3 normal, ImpactSurface surface) =>
            ImpactEffect.Spawn(impactPrefab, point, normal, surface);

        private void CompleteReload()
        {
            if (!CombatAllowed) return;
            int transferred = Mathf.Min(magazineSize - magazine, reserve);
            magazine += transferred;
            reserve -= transferred;
            reloading = false;
            ReloadFinished?.Invoke();
            RefreshHud();
        }

        private void RefreshHud() => hud?.SetAmmo(magazine, reserve, reloading);

        private void OnValidate()
        {
            roundsPerSecond = Mathf.Max(0.1f, roundsPerSecond);
            magazineSize = Mathf.Max(1, magazineSize);
            reloadDuration = Mathf.Max(0.01f, reloadDuration);
        }
    }
}

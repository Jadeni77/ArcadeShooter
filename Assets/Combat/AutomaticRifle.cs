using ArcadeShooter.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Combat
{
    [RequireComponent(typeof(PlayerInput), typeof(AudioSource))]
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
        [SerializeField] private Transform weaponModel;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private GameObject impactPrefab;
        [SerializeField] private WeaponHud hud;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float recoilDistance = 0.07f;
        [SerializeField, Min(0f)] private float recoilRecovery = 12f;
        [SerializeField, Min(0f)] private float flashDuration = 0.055f;
        [SerializeField] private AudioClip gunfireSound;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private AudioClip reloadSound;

        private InputAction attackAction;
        private InputAction reloadAction;
        private AudioSource audioSource;
        private Vector3 restingPosition;
        private int magazine;
        private int reserve;
        private float nextShotAt;
        private float reloadEndsAt;
        private float flashEndsAt;
        private bool reloading;

        public int Magazine => magazine;
        public int Reserve => reserve;
        public bool IsReloading => reloading;

        private void Awake()
        {
            PlayerInput input = GetComponent<PlayerInput>();
            attackAction = input.actions.FindAction("Attack", true);
            reloadAction = input.actions.FindAction("Reload", true);
            audioSource = GetComponent<AudioSource>();
            if (aimCamera == null)
                Debug.LogError("AutomaticRifle needs an aim camera.", this);
            if (weaponModel != null)
                restingPosition = weaponModel.localPosition;
            magazine = magazineSize;
            reserve = startingReserve;
            if (muzzleFlash != null)
                muzzleFlash.SetActive(false);
            RefreshHud();
        }

        private void Update()
        {
            if (reloading && Time.time >= reloadEndsAt)
                CompleteReload();

            if (reloadAction.WasPressedThisFrame())
                BeginReload();

            if (attackAction.IsPressed())
                TryFire();

            if (muzzleFlash != null && muzzleFlash.activeSelf && Time.time >= flashEndsAt)
                muzzleFlash.SetActive(false);
            if (weaponModel != null)
                weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, restingPosition,
                    1f - Mathf.Exp(-recoilRecovery * Time.deltaTime));
        }

        private bool TryFire()
        {
            if (reloading || magazine <= 0 || aimCamera == null || Time.time < nextShotAt)
                return false;

            magazine--;
            nextShotAt = Time.time + 1f / roundsPerSecond;
            if (weaponModel != null)
                weaponModel.localPosition = restingPosition - Vector3.forward * recoilDistance;
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                flashEndsAt = Time.time + flashDuration;
            }
            Play(gunfireSound);

            Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (impactPrefab != null)
                    Instantiate(impactPrefab, hit.point + hit.normal * 0.015f,
                        Quaternion.FromToRotation(Vector3.forward, hit.normal));
                Play(impactSound);
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.ApplyDamage(damage, hit.point))
                    hud?.ConfirmHit();
            }
            RefreshHud();
            return true;
        }

        private void BeginReload()
        {
            if (reloading || magazine >= magazineSize || reserve <= 0)
                return;
            reloading = true;
            reloadEndsAt = Time.time + reloadDuration;
            Play(reloadSound);
            RefreshHud();
        }

        private void CompleteReload()
        {
            int transferred = Mathf.Min(magazineSize - magazine, reserve);
            magazine += transferred;
            reserve -= transferred;
            reloading = false;
            RefreshHud();
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
                audioSource.PlayOneShot(clip);
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

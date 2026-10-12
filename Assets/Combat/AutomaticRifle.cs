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
        [SerializeField, Min(0f)] private float reloadLowerDistance = 0.22f;
        [SerializeField] private Vector3 reloadTilt = new Vector3(15f, 0f, -8f);
        [SerializeField, Min(0f)] private float flashDuration = 0.055f;
        [SerializeField] private AudioClip gunfireSound;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private AudioClip reloadSound;

        private InputAction attackAction;
        private InputAction reloadAction;
        private AudioSource audioSource;
        private Vector3 restingPosition;
        private Quaternion restingRotation;
        private bool poseInitialized;
        private float recoilOffset;
        private int magazine;
        private int reserve;
        private float nextShotAt;
        private float reloadStartedAt;
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
            {
                restingPosition = weaponModel.localPosition;
                restingRotation = weaponModel.localRotation;
                poseInitialized = true;
            }
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
            UpdateWeaponPose();
        }

        private bool TryFire()
        {
            if (reloading || aimCamera == null || Time.time < nextShotAt)
                return false;
            if (magazine <= 0)
            {
                BeginReload();
                return false;
            }

            magazine--;
            nextShotAt = Time.time + 1f / roundsPerSecond;
            recoilOffset = recoilDistance;
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
            if (magazine == 0)
                BeginReload();
            RefreshHud();
            return true;
        }

        private void BeginReload()
        {
            if (reloading || magazine >= magazineSize || reserve <= 0)
                return;
            reloading = true;
            reloadStartedAt = Time.time;
            reloadEndsAt = reloadStartedAt + reloadDuration;
            recoilOffset = 0f;
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

        private void UpdateWeaponPose()
        {
            if (weaponModel == null)
                return;

            recoilOffset = Mathf.Lerp(recoilOffset, 0f,
                1f - Mathf.Exp(-recoilRecovery * Time.deltaTime));

            float reloadPose = 0f;
            if (reloading)
            {
                float progress = Mathf.Clamp01((Time.time - reloadStartedAt) / reloadDuration);
                float lower = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.25f));
                float raise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.65f) / 0.35f));
                reloadPose = lower * (1f - raise);
            }

            weaponModel.localPosition = restingPosition - Vector3.forward * recoilOffset
                + Vector3.down * (reloadLowerDistance * reloadPose);
            weaponModel.localRotation = restingRotation * Quaternion.Euler(reloadTilt * reloadPose);
        }

        private void OnDisable()
        {
            if (!poseInitialized || weaponModel == null)
                return;
            weaponModel.localPosition = restingPosition;
            weaponModel.localRotation = restingRotation;
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

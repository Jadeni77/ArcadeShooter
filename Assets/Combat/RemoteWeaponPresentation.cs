using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Third-person cosmetic response to already synchronized shot/reload state.
    public sealed class RemoteWeaponPresentation : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float recoilDistance = 0.045f;
        [SerializeField, Min(0f)] private float reloadDrop = 0.12f;
        [SerializeField, Min(0f)] private float recovery = 10f;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.055f;

        private Vector3 restPosition;
        private Quaternion restRotation;
        private float recoil;
        private bool reloading;
        private float flashEndsAt;

        private void Awake()
        {
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
            if (muzzleFlash != null) muzzleFlash.SetActive(false);
        }

        public void Shot()
        {
            recoil = recoilDistance;
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                flashEndsAt = Time.time + flashDuration;
            }
        }
        public void SetReloading(bool value) => reloading = value;

        private void LateUpdate()
        {
            if (muzzleFlash != null && muzzleFlash.activeSelf && Time.time >= flashEndsAt)
                muzzleFlash.SetActive(false);
            recoil = Mathf.MoveTowards(recoil, 0f, recovery * Time.deltaTime * recoilDistance);
            Vector3 target = restPosition - Vector3.forward * recoil -
                Vector3.down * (reloading ? reloadDrop : 0f);
            Quaternion rotation = restRotation * Quaternion.Euler(reloading ? 16f : 0f, 0f, 0f);
            float blend = 1f - Mathf.Exp(-recovery * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, blend);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, blend);
        }
    }
}

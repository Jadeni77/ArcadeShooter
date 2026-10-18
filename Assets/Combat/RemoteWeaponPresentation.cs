using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Third-person cosmetic response to already synchronized shot/reload state.
    public sealed class RemoteWeaponPresentation : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float recoilDistance = 0.045f;
        [SerializeField, Min(0f)] private float reloadDrop = 0.12f;
        [SerializeField, Min(0f)] private float recovery = 10f;

        private Vector3 restPosition;
        private Quaternion restRotation;
        private float recoil;
        private bool reloading;

        private void Awake()
        {
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
        }

        public void Shot() => recoil = recoilDistance;
        public void SetReloading(bool value) => reloading = value;

        private void LateUpdate()
        {
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

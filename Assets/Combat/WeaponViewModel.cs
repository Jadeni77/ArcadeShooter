using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Cosmetic first-person pose for the rifle model: recoil kick and the reload lower/tilt.
    // It only reads AutomaticRifle state, so reload timing is unchanged if this is disabled,
    // removed, or replaced by another animation.
    public sealed class WeaponViewModel : MonoBehaviour
    {
        [SerializeField] private AutomaticRifle rifle;
        [SerializeField, Min(0f)] private float recoilDistance = 0.07f;
        [SerializeField, Min(0f)] private float recoilRecovery = 12f;
        [SerializeField, Min(0f)] private float reloadLowerDistance = 0.22f;
        [SerializeField] private Vector3 reloadTilt = new Vector3(15f, 0f, -8f);

        private Vector3 restingPosition;
        private Quaternion restingRotation;
        private float recoilOffset;

        private void Awake()
        {
            if (rifle == null)
                rifle = GetComponentInParent<AutomaticRifle>();
            if (rifle == null)
                Debug.LogError("WeaponViewModel needs an AutomaticRifle to observe.", this);
            restingPosition = transform.localPosition;
            restingRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            if (rifle != null)
                rifle.Fired += OnFired;
        }

        private void OnDisable()
        {
            if (rifle != null)
                rifle.Fired -= OnFired;
            recoilOffset = 0f;
            transform.localPosition = restingPosition;
            transform.localRotation = restingRotation;
        }

        private void OnFired() => recoilOffset = recoilDistance;

        // LateUpdate reads the rifle after its Update has started or finished a reload this frame.
        private void LateUpdate()
        {
            if (rifle == null)
                return;

            recoilOffset = rifle.IsReloading ? 0f : Mathf.Lerp(recoilOffset, 0f,
                1f - Mathf.Exp(-recoilRecovery * Time.deltaTime));
            float reloadPose = rifle.IsReloading ? ReloadPose(rifle.ReloadProgress) : 0f;

            transform.localPosition = restingPosition - Vector3.forward * recoilOffset
                + Vector3.down * (reloadLowerDistance * reloadPose);
            transform.localRotation = restingRotation * Quaternion.Euler(reloadTilt * reloadPose);
        }

        // Lowers over the first quarter of the reload and returns over the last 35%.
        private static float ReloadPose(float progress)
        {
            float lower = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.25f));
            float raise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.65f) / 0.35f));
            return lower * (1f - raise);
        }
    }
}

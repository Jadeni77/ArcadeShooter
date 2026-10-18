using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Combat
{
    // Cosmetic motion on a replaceable visual root. Rifle timing and camera aim stay independent.
    public sealed class WeaponViewModel : MonoBehaviour
    {
        [SerializeField] private AutomaticRifle rifle;
        [SerializeField] private WeaponAim aim;
        [SerializeField] private Transform magazine;
        [SerializeField] private Vector3 aimedOffset = new Vector3(-0.34f, 0.19f, -0.1f);
        [SerializeField, Min(0f)] private float recoilDistance = 0.07f;
        [SerializeField, Min(0f)] private float recoilRecovery = 12f;
        [SerializeField, Min(0f)] private float reloadLowerDistance = 0.22f;
        [SerializeField] private Vector3 reloadTilt = new Vector3(15f, 0f, -8f);
        [SerializeField, Min(0f)] private float swayAmount = 0.0012f;
        [SerializeField, Min(0f)] private float bobAmount = 0.012f;
        [SerializeField, Min(0f)] private float bobFrequency = 9f;
        [SerializeField] private Vector3 sprintOffset = new Vector3(0.04f, -0.09f, -0.08f);
        [SerializeField, Min(0f)] private float poseSpeed = 14f;

        private Vector3 restingPosition;
        private Quaternion restingRotation;
        private Vector3 magazinePosition;
        private InputAction lookAction;
        private InputAction sprintAction;
        private CharacterController controller;
        private float recoilOffset;
        private float bobPhase;
        private Vector3 smoothPosition;
        private Quaternion smoothRotation;

        private void Awake()
        {
            if (rifle == null) rifle = GetComponentInParent<AutomaticRifle>();
            if (aim == null) aim = GetComponentInParent<WeaponAim>();
            if (magazine == null) magazine = transform.Find("Magazine");
            PlayerInput input = GetComponentInParent<PlayerInput>();
            if (input != null)
            {
                lookAction = input.actions.FindAction("Look");
                sprintAction = input.actions.FindAction("Sprint");
            }
            controller = GetComponentInParent<CharacterController>();
            restingPosition = smoothPosition = transform.localPosition;
            restingRotation = smoothRotation = transform.localRotation;
            if (magazine != null) magazinePosition = magazine.localPosition;
        }

        private void OnEnable()
        {
            if (rifle != null) rifle.Fired += OnFired;
        }

        private void OnDisable()
        {
            if (rifle != null) rifle.Fired -= OnFired;
            recoilOffset = bobPhase = 0f;
            transform.localPosition = smoothPosition = restingPosition;
            transform.localRotation = smoothRotation = restingRotation;
            if (magazine != null) magazine.localPosition = magazinePosition;
        }

        private void OnFired() => recoilOffset = recoilDistance;

        private void LateUpdate()
        {
            if (rifle == null) return;

            float delta = Time.deltaTime;
            recoilOffset = Mathf.MoveTowards(recoilOffset, 0f, recoilRecovery * delta * recoilDistance);
            float reload = rifle.IsReloading ? ReloadPose(rifle.ReloadProgress) : 0f;
            float aimAmount = aim != null ? aim.Amount : 0f;
            bool sprinting = sprintAction != null && sprintAction.IsPressed() && aimAmount < 0.1f;
            float horizontalSpeed = controller != null ?
                Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude : 0f;
            float moving = Mathf.Clamp01(horizontalSpeed / 5f);
            if (moving > 0.01f) bobPhase += delta * bobFrequency * (sprinting ? 1.4f : 1f);
            Vector2 look = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
            Vector3 sway = new Vector3(-look.x, -look.y, 0f) * swayAmount;
            sway = Vector3.ClampMagnitude(sway, 0.035f);
            Vector3 bob = new Vector3(Mathf.Sin(bobPhase) * 0.5f,
                Mathf.Abs(Mathf.Cos(bobPhase)) - 0.5f, 0f) * (bobAmount * moving);
            float presentationScale = 1f - aimAmount * 0.8f;

            Vector3 targetPosition = restingPosition + aimedOffset * aimAmount +
                (sprinting ? sprintOffset : Vector3.zero) +
                (sway + bob) * presentationScale - Vector3.forward * recoilOffset -
                Vector3.down * (reloadLowerDistance * reload);
            Quaternion targetRotation = restingRotation * Quaternion.Euler(reloadTilt * reload);
            float blend = 1f - Mathf.Exp(-poseSpeed * delta);
            smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, blend);
            smoothRotation = Quaternion.Slerp(smoothRotation, targetRotation, blend);
            transform.localPosition = smoothPosition;
            transform.localRotation = smoothRotation;

            if (magazine != null)
            {
                float pull = rifle.IsReloading ? MagazinePose(rifle.ReloadProgress) : 0f;
                magazine.localPosition = magazinePosition + Vector3.down * (0.22f * pull);
            }
        }

        private static float ReloadPose(float progress)
        {
            float lower = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.22f));
            float raise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.76f) / 0.24f));
            return lower * (1f - raise);
        }

        private static float MagazinePose(float progress)
        {
            float remove = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.28f) / 0.18f));
            float insert = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.62f) / 0.2f));
            return remove * (1f - insert);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Combat
{
    // Local camera and view-model presentation only. Hitscan still uses the camera ray.
    [RequireComponent(typeof(PlayerInput))]
    public sealed class WeaponAim : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField, Range(20f, 90f)] private float aimedFieldOfView = 43f;
        [SerializeField, Min(0.1f)] private float transitionSpeed = 12f;
        [SerializeField, Range(0.1f, 1f)] private float sensitivityMultiplier = 0.65f;

        private InputAction aimAction;
        private float normalFieldOfView;
        private float amount;

        public float Amount => amount;
        public float SensitivityScale => Mathf.Lerp(1f, sensitivityMultiplier, amount);

        private void Awake()
        {
            aimAction = GetComponent<PlayerInput>().actions.FindAction("Aim", true);
            if (aimCamera != null) normalFieldOfView = aimCamera.fieldOfView;
        }

        private void LateUpdate()
        {
            bool aiming = Cursor.lockState == CursorLockMode.Locked && aimAction.IsPressed();
            float target = aiming ? 1f : 0f;
            amount = Mathf.MoveTowards(amount, target, transitionSpeed * Time.deltaTime);
            if (aimCamera != null)
                aimCamera.fieldOfView = Mathf.Lerp(normalFieldOfView, aimedFieldOfView, amount);
        }

        private void OnDisable()
        {
            amount = 0f;
            if (aimCamera != null && normalFieldOfView > 0f)
                aimCamera.fieldOfView = normalFieldOfView;
        }
    }
}

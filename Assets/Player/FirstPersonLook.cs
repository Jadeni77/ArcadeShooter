using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Range(1f, 89f)] private float maxPitch = 85f;

        private InputAction lookAction;
        private float pitch;

        private void Awake()
        {
            lookAction = GetComponent<PlayerInput>().actions.FindAction("Look", true);
            if (cameraPivot == null)
                Debug.LogError("FirstPersonLook needs a camera pivot.", this);
        }

        private void Start()
        {
            if (cameraPivot != null)
                pitch = Mathf.DeltaAngle(0f, cameraPivot.localEulerAngles.x);
            LockCursor();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                UnlockCursor();

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    LockCursor();
                return;
            }

            if (cameraPivot == null)
                return;

            Vector2 delta = lookAction.ReadValue<Vector2>() * mouseSensitivity;
            transform.Rotate(Vector3.up, delta.x, Space.Self);
            pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void OnDisable()
        {
            UnlockCursor();
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

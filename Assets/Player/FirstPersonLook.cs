using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
            {
                UnlockCursor();
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame &&
                    !IsPointerOverUi())
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

        private static bool IsPointerOverUi()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || Mouse.current == null)
                return false;
            PointerEventData pointer = new PointerEventData(eventSystem)
            {
                position = Mouse.current.position.ReadValue()
            };
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(pointer, hits);
            return hits.Count > 0;
        }
    }
}

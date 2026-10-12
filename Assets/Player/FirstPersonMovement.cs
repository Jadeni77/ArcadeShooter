using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public sealed class FirstPersonMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 8f;
        [SerializeField, Min(0f)] private float acceleration = 30f;
        [SerializeField, Min(0f)] private float airAcceleration = 10f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.25f;
        [SerializeField, Min(0f)] private float gravity = 24f;

        private CharacterController characterController;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction jumpAction;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            PlayerInput input = GetComponent<PlayerInput>();
            moveAction = input.actions.FindAction("Move", true);
            sprintAction = input.actions.FindAction("Sprint", true);
            jumpAction = input.actions.FindAction("Jump", true);
        }

        private void Update()
        {
            // The controller can be disabled on remote player objects when ownership is added.
            Vector2 moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
            Vector3 desiredDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            float speed = sprintAction.IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 desiredVelocity = desiredDirection * speed;
            bool grounded = IsGrounded();
            float currentAcceleration = grounded ? acceleration : airAcceleration;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity, desiredVelocity, currentAcceleration * Time.deltaTime);

            if (grounded && verticalVelocity <= 0f)
            {
                // A small downward velocity keeps the controller touching uneven floors.
                if (verticalVelocity < 0f)
                    verticalVelocity = -2f;

                if (jumpAction.WasPressedThisFrame())
                    verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
            }

            verticalVelocity -= gravity * Time.deltaTime;
            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            CollisionFlags collisions = characterController.Move(motion * Time.deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
        }

        private bool IsGrounded()
        {
            if (characterController.isGrounded)
                return true;

            // CharacterController can briefly report false while resting within its skin width.
            Vector3 center = transform.TransformPoint(characterController.center);
            float probeLength = characterController.height * 0.5f + 0.2f;
            return Physics.Raycast(center, Vector3.down, probeLength,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }
}

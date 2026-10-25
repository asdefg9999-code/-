using UnityEngine;

namespace ImmortalOrgan
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Third-person locomotion")]
        public float MoveSpeed = 5.1f;
        public float SprintSpeed = 7.3f;
        public float RotationSharpness = 13f;
        public float JumpHeight = 1.65f;
        public float DashSpeed = 18f;
        public float DashDuration = 0.23f;
        public float DashCooldown = 1.15f;

        public Vector2 MobileMove { get; set; }
        public bool IsDashing => dashTimer > 0f;
        public bool IsMoving { get; private set; }
        public float MoveCycle { get; private set; }

        private CharacterController controller;
        private OrbitCamera orbit;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private float dashTimer;
        private float dashCooldownTimer;
        private float dashDirectionFactor;
        private Vector3 dashDirection;
        private bool jumpRequested;
        private bool dashRequested;

        public void SetCamera(OrbitCamera cameraController) => orbit = cameraController;
        public void RequestJump() => jumpRequested = true;
        public void RequestDash() => dashRequested = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = 1.82f;
            controller.center = new Vector3(0, 0.91f, 0);
            controller.radius = 0.33f;
            controller.stepOffset = 0.30f;
            controller.slopeLimit = 55f;
            controller.skinWidth = 0.035f;
        }

        private void Update()
        {
            if (GameRuntime.Instance == null || !GameRuntime.Instance.Playing) return;

            float dt = Time.deltaTime;
            dashCooldownTimer = Mathf.Max(0, dashCooldownTimer - dt);
            dashTimer = Mathf.Max(0, dashTimer - dt);

            var keyboard = new Vector2(
                (GameInput.Held(KeyCode.D) || GameInput.Held(KeyCode.RightArrow) ? 1f : 0f) -
                (GameInput.Held(KeyCode.A) || GameInput.Held(KeyCode.LeftArrow) ? 1f : 0f),
                (GameInput.Held(KeyCode.W) || GameInput.Held(KeyCode.UpArrow) ? 1f : 0f) -
                (GameInput.Held(KeyCode.S) || GameInput.Held(KeyCode.DownArrow) ? 1f : 0f));

            Vector2 axis = Vector2.ClampMagnitude(keyboard + MobileMove, 1f);
            var forward = orbit != null ? orbit.FlatForward : Vector3.forward;
            var right = orbit != null ? orbit.FlatRight : Vector3.right;
            Vector3 desired = Vector3.ClampMagnitude(right * axis.x + forward * axis.y, 1f);

            bool grounded = controller.isGrounded;
            if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;

            if ((jumpRequested || GameInput.Pressed(KeyCode.Space)) && grounded && dashTimer <= 0f)
            {
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y);
            }

            if ((dashRequested || GameInput.Pressed(KeyCode.LeftShift)) &&
                dashCooldownTimer <= 0f && GameRuntime.Instance.Combat.TrySpendDash(8))
            {
                dashDirection = desired.sqrMagnitude > 0.01f ? desired.normalized : transform.forward;
                dashTimer = DashDuration;
                dashCooldownTimer = DashCooldown;
                dashDirectionFactor = 1f;
                GameRuntime.Instance.Combat.GrantInvulnerability(0.35f);
            }

            jumpRequested = false;
            dashRequested = false;

            if (dashTimer > 0f)
            {
                planarVelocity = dashDirection * DashSpeed * dashDirectionFactor;
            }
            else
            {
                float speed = GameInput.Held(KeyCode.LeftControl) ? SprintSpeed : MoveSpeed;
                planarVelocity = Vector3.Lerp(planarVelocity, desired * speed, 1f - Mathf.Exp(-15f * dt));
            }

            verticalVelocity += Physics.gravity.y * dt;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);

            Vector3 lookDirection = desired.sqrMagnitude > 0.02f ? desired :
                dashTimer > 0f ? dashDirection : Vector3.zero;

            if (lookDirection.sqrMagnitude > 0.02f)
            {
                Quaternion look = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look,
                    1f - Mathf.Exp(-RotationSharpness * dt));
            }

            IsMoving = axis.sqrMagnitude > 0.04f || dashTimer > 0f;
            if (IsMoving) MoveCycle += dt * (dashTimer > 0f ? 15f : 10f);
        }
    }
}

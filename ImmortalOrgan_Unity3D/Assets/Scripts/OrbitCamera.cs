using UnityEngine;

namespace ImmortalOrgan
{
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 5.7f;
        public float Pitch = 24f;
        public float Yaw = 20f;
        public float FollowSharpness = 10f;

        public Vector3 FlatForward
        {
            get
            {
                Vector3 v = Quaternion.Euler(0f, Yaw, 0f) * Vector3.forward;
                return v.normalized;
            }
        }

        public Vector3 FlatRight => Quaternion.Euler(0f, Yaw, 0f) * Vector3.right;

        private Vector2 touchOrbit;
        private bool initialized;

        public void AddTouchOrbit(Vector2 pixelDelta)
        {
            touchOrbit += pixelDelta;
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            if (GameInput.MouseRightHeld())
            {
                Vector2 mouse = GameInput.MouseDelta();
                Yaw += mouse.x * 4.0f;
                Pitch -= mouse.y * 2.9f;
            }

            Yaw += touchOrbit.x * 0.16f;
            Pitch -= touchOrbit.y * 0.12f;
            touchOrbit = Vector2.zero;
            Pitch = Mathf.Clamp(Pitch, 10f, 64f);
            Distance = Mathf.Clamp(Distance - GameInput.Scroll() * 3f, 3.5f, 9f);

            Vector3 anchor = Target.position + new Vector3(0f, 1.45f, 0f);
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0f);
            Vector3 viewDirection = rotation * Vector3.back;
            float radius = 0.19f;
            float cameraDistance = Distance;

            // Player geometry is assigned layer 8, excluded to prevent self-collision.
            const int ignorePlayer = ~(1 << 8);
            if (Physics.SphereCast(anchor, radius, viewDirection, out RaycastHit hit,
                    Distance, ignorePlayer, QueryTriggerInteraction.Ignore))
            {
                cameraDistance = Mathf.Clamp(hit.distance - 0.12f, 1f, Distance);
            }

            Vector3 targetPosition = anchor + viewDirection * cameraDistance;
            if (!initialized)
            {
                transform.position = targetPosition;
                initialized = true;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition,
                    1f - Mathf.Exp(-FollowSharpness * Time.deltaTime));
            }

            transform.rotation = Quaternion.LookRotation(anchor - transform.position, Vector3.up);
        }
    }
}

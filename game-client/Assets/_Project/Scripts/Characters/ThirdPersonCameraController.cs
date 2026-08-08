using LegadoPeru.Core;
using LegadoPeru.InputSystem;
using LegadoPeru.Settings;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Cámara en tercera persona (prompt §10): orientación libre, seguimiento suave y
    /// evasión simple de colisión contra el mundo mediante Linecast. Sin cámara
    /// cinematográfica compleja en esta fase.
    /// </summary>
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float heightOffset = 1.6f;
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float followSmoothTime = 0.08f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float initialYaw;
        [SerializeField] private float initialPitch = 15f;

        private float yaw;
        private float pitch;
        private Vector3 followVelocity;
        private float sensitivity = 1f;

        private void Awake()
        {
            yaw = initialYaw;
            pitch = initialPitch;
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out SettingsService settings))
                sensitivity = settings.Data.cameraSensitivity;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null || !ServiceLocator.TryGet(out GameInput input)) return;

            Vector2 look = input.LookDelta * sensitivity;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * heightOffset;
            Vector3 desiredPosition = pivot - rotation * Vector3.forward * distance;

            float finalDistance = distance;
            if (Physics.Linecast(pivot, desiredPosition, out RaycastHit hit, collisionMask))
                finalDistance = Mathf.Clamp(hit.distance * 0.9f, 0.5f, distance);

            Vector3 targetPosition = pivot - rotation * Vector3.forward * finalDistance;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, followSmoothTime);
            transform.rotation = rotation;
        }
    }
}

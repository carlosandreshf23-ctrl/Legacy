using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Envoltorio de CharacterController: gravedad, colisiones, aceleración/desaceleración
    /// y rotación hacia la dirección de movimiento (prompt §7-8). Sin saltos ni parkour:
    /// el personaje es una persona normal explorando, no un avatar arcade.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField] private MovementConfig config;

        private CharacterController controller;
        private float verticalVelocity;

        private const float Gravity = -9.81f;

        public float CurrentSpeed { get; private set; }
        public bool IsGrounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void SetConfig(MovementConfig movementConfig) => config = movementConfig;

        /// <summary>worldDirection es un vector de movimiento en espacio mundo (no normalizado por el caller).</summary>
        public void Move(Vector3 worldDirection, bool wantsRun, float deltaTime)
        {
            bool hasInput = worldDirection.sqrMagnitude > 0.0001f;
            float targetSpeed = hasInput ? (wantsRun ? config.runSpeed : config.walkSpeed) : 0f;

            float rate = targetSpeed > CurrentSpeed ? config.acceleration : config.deceleration;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, targetSpeed, rate * deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -1f;
            verticalVelocity += Gravity * deltaTime;

            Vector3 planarDirection = hasInput ? worldDirection.normalized : Vector3.zero;
            Vector3 motion = planarDirection * CurrentSpeed;
            motion.y = verticalVelocity;
            controller.Move(motion * deltaTime);

            if (hasInput)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(worldDirection.x, 0f, worldDirection.z));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, config.rotationSpeedDegPerSec * deltaTime);
            }
        }
    }
}

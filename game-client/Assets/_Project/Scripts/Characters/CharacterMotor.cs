using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Motor de movimiento 2D cenital (Fase 2, prompt §56: movimiento LIBRE sobre mapa de
    /// tiles, no grid-based). Reemplaza la versión 3D de Fase 1 basada en CharacterController
    /// (eliminada junto con la cámara en tercera persona, prompt Fase 2 §16).
    ///
    /// Usa un Rigidbody2D Kinematic movido con MovePosition: nos da colisión solida contra
    /// paredes/objetos sin sufrir físicas de rebote no deseadas en un RPG top-down.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField] private MovementConfig config;

        private Rigidbody2D body;
        private Vector2 currentVelocity;

        public Vector2 FacingDirection { get; private set; } = Vector2.down;
        public float CurrentSpeed => currentVelocity.magnitude;
        public bool IsMoving => CurrentSpeed > 0.05f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        public void SetConfig(MovementConfig movementConfig) => config = movementConfig;

        public void SetFacing(Vector2 direction) => FacingDirection = SnapToCardinal(direction);

        /// <summary>moveInput: vector de entrada en rango [-1,1] por eje (ya normalizado por el caller si aplica).</summary>
        public void Move(Vector2 moveInput, bool wantsRun, float deltaTime)
        {
            bool hasInput = moveInput.sqrMagnitude > 0.0001f;
            Vector2 direction = hasInput ? moveInput.normalized : Vector2.zero;
            float targetSpeed = hasInput ? (wantsRun ? config.runSpeed : config.walkSpeed) : 0f;
            Vector2 targetVelocity = direction * targetSpeed;

            float rate = targetSpeed > currentVelocity.magnitude ? config.acceleration : config.deceleration;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * deltaTime);

            body.MovePosition(body.position + currentVelocity * deltaTime);

            if (hasInput)
                FacingDirection = SnapToCardinal(direction);
        }

        /// <summary>Reduce la direccion continua a las 4 direcciones cardinales que tiene sprite (down/up/left/right).</summary>
        private static Vector2 SnapToCardinal(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? Vector2.right : Vector2.left;
            return direction.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}

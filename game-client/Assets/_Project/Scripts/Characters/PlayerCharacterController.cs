using System;
using LegadoPeru.Core;
using LegadoPeru.InputSystem;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>DTO de posición/rotación del jugador para SaveSystem (nunca se guarda una referencia a GameObject).</summary>
    [Serializable]
    public class PlayerStateDto
    {
        public float posX, posY, posZ;
        public float rotY;
    }

    /// <summary>
    /// Personaje controlable (prompt §7). Representa técnicamente a Mateo Salazar como
    /// prototipo (prompt §44-45): los identificadores narrativos están marcados PROTOTYPE
    /// y no deben considerarse datos históricos definitivos todavía.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("Identidad de prototipo (ver 06_FAMILY_SYSTEM.md) — PROTOTYPE / NOT VALIDATED")]
        public string characterId = "SAL_MATEO_001";
        public string familyId = "FAM_SALAZAR";
        public string displayName = "Mateo Salazar";
        public string historicalAccuracyStatus = "PROTOTYPE / NOT VALIDATED";

        [Header("Config")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 18f;
        [SerializeField] private float staminaRecoveryRate = 12f;

        private CharacterMotor motor;
        private StaminaSystem stamina;
        private Transform cameraTransform;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            stamina = new StaminaSystem(maxStamina, staminaDrainRate, staminaRecoveryRate);
            stamina.OnStaminaChanged += (current, max) => EventBus.Publish(new StaminaChangedEvent(current, max));

            ServiceLocator.Register(this);
        }

        private void Start()
        {
            var mainCamera = Camera.main;
            cameraTransform = mainCamera != null ? mainCamera.transform : null;
        }

        private void Update()
        {
            if (!ServiceLocator.TryGet(out GameInput input)) return;

            Vector2 move = input.MoveAxis;
            bool wantsRun = input.RunHeld && stamina.CanRun;

            Vector3 forward = cameraTransform != null
                ? Vector3.Scale(cameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized
                : Vector3.forward;
            Vector3 right = cameraTransform != null
                ? Vector3.Scale(cameraTransform.right, new Vector3(1f, 0f, 1f)).normalized
                : Vector3.right;

            Vector3 direction = forward * move.y + right * move.x;

            motor.Move(direction, wantsRun, Time.deltaTime);
            stamina.Tick(Time.deltaTime, wantsRun && direction.sqrMagnitude > 0.0001f);
        }

        public PlayerStateDto CaptureState() => new PlayerStateDto
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            rotY = transform.eulerAngles.y
        };

        public void RestoreState(PlayerStateDto dto)
        {
            if (dto == null) return;

            var controller = GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null) controller.enabled = false;

            transform.position = new Vector3(dto.posX, dto.posY, dto.posZ);
            transform.rotation = Quaternion.Euler(0f, dto.rotY, 0f);

            if (controller != null) controller.enabled = wasEnabled;
        }
    }
}

using System;
using LegadoPeru.Core;
using LegadoPeru.InputSystem;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>DTO de posición/orientación del jugador para SaveSystem (nunca se guarda una referencia a GameObject).</summary>
    [Serializable]
    public class PlayerStateDto
    {
        public float posX, posY, posZ;

        /// <summary>Dirección hacia la que miraba el personaje al guardar: "down"/"left"/"up"/"right".</summary>
        public string facing = "down";
    }

    /// <summary>
    /// Personaje controlable (prompt §7, adaptado a RPG 2D cenital en Fase 2 — prompt Fase 2
    /// §16/§56). Representa técnicamente a Mateo Salazar como prototipo (prompt §44-45): los
    /// identificadores narrativos están marcados PROTOTYPE y no deben considerarse datos
    /// históricos definitivos todavía.
    ///
    /// Movimiento LIBRE sobre el mapa de tiles (no grid-based): el input se aplica
    /// directamente en espacio de mundo XY, sin relación a la cámara (la cámara es cenital
    /// fija, no orbita, así que "arriba" en pantalla siempre es +Y de mundo).
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

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            stamina = new StaminaSystem(maxStamina, staminaDrainRate, staminaRecoveryRate);
            stamina.OnStaminaChanged += (current, max) => EventBus.Publish(new StaminaChangedEvent(current, max));

            ServiceLocator.Register(this);
        }

        private void Update()
        {
            if (!ServiceLocator.TryGet(out GameInput input)) return;

            Vector2 move = input.MoveAxis;
            bool wantsRun = input.RunHeld && stamina.CanRun;

            motor.Move(move, wantsRun, Time.deltaTime);
            stamina.Tick(Time.deltaTime, wantsRun && move.sqrMagnitude > 0.0001f);
        }

        public PlayerStateDto CaptureState() => new PlayerStateDto
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            facing = FacingToString(motor.FacingDirection)
        };

        public void RestoreState(PlayerStateDto dto)
        {
            if (dto == null) return;
            transform.position = new Vector3(dto.posX, dto.posY, dto.posZ);
            motor.SetFacing(StringToFacing(dto.facing));
        }

        private static Vector2 StringToFacing(string facing) => facing switch
        {
            "left" => Vector2.left,
            "right" => Vector2.right,
            "up" => Vector2.up,
            _ => Vector2.down
        };

        private static string FacingToString(Vector2 facing)
        {
            if (facing == Vector2.left) return "left";
            if (facing == Vector2.right) return "right";
            if (facing == Vector2.up) return "up";
            return "down";
        }
    }
}

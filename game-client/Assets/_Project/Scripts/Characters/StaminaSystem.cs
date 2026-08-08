using System;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Sistema básico de resistencia (prompt §9). Afecta solo a correr; se recupera al dejar
    /// de correr. Deliberadamente NO incluye hambre/sed/fatiga/temperatura en esta fase.
    /// </summary>
    public class StaminaSystem
    {
        public float MaxStamina { get; }
        public float Current { get; private set; }

        private readonly float drainRate;
        private readonly float recoveryRate;

        public event Action<float, float> OnStaminaChanged;

        public StaminaSystem(float maxStamina, float drainRate, float recoveryRate)
        {
            MaxStamina = maxStamina;
            Current = maxStamina;
            this.drainRate = drainRate;
            this.recoveryRate = recoveryRate;
        }

        public bool CanRun => Current > 0f;

        public void Tick(float deltaTime, bool isRunningWithMovement)
        {
            float previous = Current;
            Current = isRunningWithMovement
                ? Mathf.Max(0f, Current - drainRate * deltaTime)
                : Mathf.Min(MaxStamina, Current + recoveryRate * deltaTime);

            if (!Mathf.Approximately(previous, Current))
                OnStaminaChanged?.Invoke(Current, MaxStamina);
        }
    }
}

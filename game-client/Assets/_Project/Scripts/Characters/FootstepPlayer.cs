using LegadoPeru.Audio;
using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>Dispara sonido de pasos (placeholder, prompt §47) en función de la velocidad actual del motor.</summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private float baseStepInterval = 0.45f;

        private float timer;
        private CharacterMotor motor;

        private void Awake() => motor = GetComponent<CharacterMotor>();

        private void Update()
        {
            if (motor == null || motor.CurrentSpeed < 0.1f)
            {
                timer = 0f;
                return;
            }

            timer += Time.deltaTime;
            float interval = baseStepInterval / Mathf.Max(0.3f, motor.CurrentSpeed / 1.4f);
            if (timer < interval) return;

            timer = 0f;
            if (ServiceLocator.TryGet(out AudioService audio))
                audio.PlayFootstep();
        }
    }
}

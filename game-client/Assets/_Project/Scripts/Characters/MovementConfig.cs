using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Valores de movimiento configurables (prompt §8), nunca hardcodeados en el controlador.
    /// Valores por defecto pensados para una persona normal, no un movimiento arcade:
    /// caminar ~1.4 m/s (paso humano habitual), correr ~3.2 m/s (trote sostenido corto).
    /// </summary>
    [CreateAssetMenu(menuName = "Legado/Movement Config", fileName = "MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        [Tooltip("Metros por segundo caminando.")]
        public float walkSpeed = 1.4f;

        [Tooltip("Metros por segundo corriendo.")]
        public float runSpeed = 3.2f;

        [Tooltip("Grados por segundo al rotar hacia la dirección de movimiento.")]
        public float rotationSpeedDegPerSec = 360f;

        [Tooltip("Tasa de aceleración (m/s^2) al alcanzar la velocidad objetivo.")]
        public float acceleration = 8f;

        [Tooltip("Tasa de desaceleración (m/s^2) al detenerse.")]
        public float deceleration = 10f;
    }
}

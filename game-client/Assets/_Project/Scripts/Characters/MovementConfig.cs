using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Valores de movimiento configurables (prompt §8), nunca hardcodeados en el controlador.
    /// Valores por defecto pensados para una persona normal, no un movimiento arcade:
    /// caminar ~1.4 m/s (paso humano habitual), correr ~3.2 m/s (trote sostenido corto).
    ///
    /// Desde Fase 2 (pivote a RPG 2D cenital, prompt Fase 2 §16/§56) el movimiento es libre
    /// sobre el mapa de tiles, no rotación de transform: la dirección se comunica al jugador
    /// mediante SpriteDirectionAnimator, no girando el objeto. Por eso ya no existe un campo
    /// de velocidad de rotación aquí (existía en la versión 3D de Fase 1).
    /// </summary>
    [CreateAssetMenu(menuName = "Legado/Movement Config", fileName = "MovementConfig")]
    public class MovementConfig : ScriptableObject
    {
        [Tooltip("Unidades por segundo caminando (1 unidad = 1 tile, ver ART_BIBLE_v0.1.md).")]
        public float walkSpeed = 2.2f;

        [Tooltip("Unidades por segundo corriendo.")]
        public float runSpeed = 4.2f;

        [Tooltip("Tasa de aceleración (unidades/s^2) al alcanzar la velocidad objetivo.")]
        public float acceleration = 24f;

        [Tooltip("Tasa de desaceleración (unidades/s^2) al detenerse.")]
        public float deceleration = 28f;
    }
}

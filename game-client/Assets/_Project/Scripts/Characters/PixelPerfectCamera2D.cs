using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Cámara 2D cenital fija (prompt Fase 2 §16-17): sigue al jugador suavemente, no rota,
    /// no cambia de perspectiva. Reemplaza definitivamente a la cámara en tercera persona de
    /// Fase 1. Fuerza escalado entero (pixel-perfect, prompt §17): el tamaño ortográfico se
    /// calcula a partir de PixelsPerUnit y una resolución interna de referencia, y la
    /// posición final se redondea al grid de píxeles para evitar temblor/blur de subpíxel.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PixelPerfectCamera2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private int pixelsPerUnit = 16;
        [SerializeField] private int referenceHeightPx = 180; // ver ART_BIBLE_v0.1.md §18 (resolucion interna)
        [SerializeField] private float followSmoothTime = 0.06f;

        private Camera cam;
        private Vector3 followVelocity;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            ApplyOrthographicSize();
        }

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void ApplyOrthographicSize()
        {
            // orthographicSize = mitad de la altura visible, en unidades de mundo.
            cam.orthographicSize = (referenceHeightPx / (float)pixelsPerUnit) / 2f;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime);

            // Redondeo al grid de pixeles (pixel-perfect): evita que los tiles/sprites se vean
            // con "temblor" o desalineados al mover la camara con valores fraccionarios.
            float unitsPerPixel = 1f / pixelsPerUnit;
            smoothed.x = Mathf.Round(smoothed.x / unitsPerPixel) * unitsPerPixel;
            smoothed.y = Mathf.Round(smoothed.y / unitsPerPixel) * unitsPerPixel;

            transform.position = smoothed;
            // La cámara nunca rota (prompt §16): no se toca transform.rotation aquí, salvo la
            // identidad fijada una vez en Awake por composición de escena (mirando -Z).
        }
    }
}

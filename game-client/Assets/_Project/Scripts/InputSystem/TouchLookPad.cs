using UnityEngine;
using UnityEngine.EventSystems;

namespace LegadoPeru.InputSystem
{
    /// <summary>Zona táctil derecha (control de cámara), prompt §11. Acumula delta de arrastre por frame.</summary>
    public class TouchLookPad : MonoBehaviour, IDragHandler
    {
        [SerializeField] private float sensitivity = 0.15f;

        private Vector2 accumulatedDelta;

        public void OnDrag(PointerEventData eventData)
        {
            accumulatedDelta += eventData.delta * sensitivity;
        }

        /// <summary>Debe llamarse como máximo una vez por frame; resetea el acumulador.</summary>
        public Vector2 ConsumeDelta()
        {
            var value = accumulatedDelta;
            accumulatedDelta = Vector2.zero;
            return value;
        }
    }
}

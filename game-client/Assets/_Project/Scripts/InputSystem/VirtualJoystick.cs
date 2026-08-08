using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegadoPeru.InputSystem
{
    /// <summary>Joystick táctil izquierdo (movimiento), prompt §11. UGUI puro, sin paquetes extra.</summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 90f;

        public Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out var localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            Value = clamped / handleRange;

            if (handle != null) handle.anchoredPosition = clamped;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Value = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }

        public void Configure(RectTransform backgroundRect, RectTransform handleRect, float range)
        {
            background = backgroundRect;
            handle = handleRect;
            handleRange = range;
        }
    }
}

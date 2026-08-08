using UnityEngine;

namespace LegadoPeru.InputSystem
{
    /// <summary>
    /// Implementación PC del Input Manager clásico ("Horizontal"/"Vertical" son ejes por
    /// defecto de cualquier proyecto Unity nuevo). Decisión técnica de Fase 1: se usa el
    /// Input Manager clásico en vez del paquete Input System para minimizar dependencias de
    /// assets generados por Editor; el gameplay ya está abstraído detrás de IInputProvider,
    /// por lo que migrar al paquete nuevo no requiere tocar ningún otro sistema.
    /// </summary>
    public class KeyboardMouseInputProvider : IInputProvider
    {
        private const float MouseSensitivityMultiplier = 3f;

        public Vector2 GetMoveAxis() =>
            new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));

        public Vector2 GetLookDelta() =>
            new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y")) * MouseSensitivityMultiplier;

        public bool GetInteractDown() => UnityEngine.Input.GetKeyDown(KeyCode.E);

        public bool GetRunHeld() => UnityEngine.Input.GetKey(KeyCode.LeftShift);

        public bool GetInventoryToggleDown() => UnityEngine.Input.GetKeyDown(KeyCode.Tab);

        public bool GetPauseDown() => UnityEngine.Input.GetKeyDown(KeyCode.Escape);
    }
}

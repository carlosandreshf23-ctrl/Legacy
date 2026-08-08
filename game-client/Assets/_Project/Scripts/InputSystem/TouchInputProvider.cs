using UnityEngine;

namespace LegadoPeru.InputSystem
{
    /// <summary>
    /// Implementación móvil: joystick virtual (movimiento) + zona táctil (cámara) + botones de
    /// UI para interactuar/correr/inventario/pausa. Los botones de UI llaman a los métodos
    /// Queue*/Set* desde OnClick (ver GameInput y el builder de escena de Fase 1).
    /// </summary>
    public class TouchInputProvider : IInputProvider
    {
        private VirtualJoystick moveJoystick;
        private TouchLookPad lookPad;

        private bool interactQueued;
        private bool runHeld;
        private bool inventoryToggleQueued;
        private bool pauseQueued;

        public void Configure(VirtualJoystick joystick, TouchLookPad pad)
        {
            moveJoystick = joystick;
            lookPad = pad;
        }

        public void QueueInteract() => interactQueued = true;
        public void SetRunHeld(bool held) => runHeld = held;
        public void QueueInventoryToggle() => inventoryToggleQueued = true;
        public void QueuePause() => pauseQueued = true;

        public Vector2 GetMoveAxis() => moveJoystick != null ? moveJoystick.Value : Vector2.zero;

        public Vector2 GetLookDelta() => lookPad != null ? lookPad.ConsumeDelta() : Vector2.zero;

        public bool GetInteractDown()
        {
            bool value = interactQueued;
            interactQueued = false;
            return value;
        }

        public bool GetRunHeld() => runHeld;

        public bool GetInventoryToggleDown()
        {
            bool value = inventoryToggleQueued;
            inventoryToggleQueued = false;
            return value;
        }

        public bool GetPauseDown()
        {
            bool value = pauseQueued;
            pauseQueued = false;
            return value;
        }
    }
}

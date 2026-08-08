using UnityEngine;

namespace LegadoPeru.InputSystem
{
    /// <summary>
    /// Abstracción de entrada (prompt §12). El gameplay nunca consulta teclas/toques directamente,
    /// solo estas acciones. Permite añadir Gamepad más adelante sin tocar gameplay.
    /// </summary>
    public interface IInputProvider
    {
        Vector2 GetMoveAxis();
        Vector2 GetLookDelta();
        bool GetInteractDown();
        bool GetRunHeld();
        bool GetInventoryToggleDown();
        bool GetPauseDown();
    }
}

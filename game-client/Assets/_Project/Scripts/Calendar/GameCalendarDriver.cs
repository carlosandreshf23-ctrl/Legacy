using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.Calendar
{
    /// <summary>Componente mínimo que hace avanzar GameCalendarSystem cada frame. Debe ejecutarse temprano.</summary>
    [DefaultExecutionOrder(-200)]
    public class GameCalendarDriver : MonoBehaviour
    {
        private void Update()
        {
            if (ServiceLocator.TryGet(out GameCalendarSystem calendar))
                calendar.Tick(Time.unscaledDeltaTime);
        }
    }
}

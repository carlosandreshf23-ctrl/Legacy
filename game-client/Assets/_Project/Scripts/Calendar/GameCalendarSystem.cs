using System;
using UnityEngine;

namespace LegadoPeru.Calendar
{
    /// <summary>
    /// Primera implementación real de GameCalendarSystem (05_TIME_SYSTEM.md §1).
    /// Fuente única de verdad sobre la fecha de juego. En el Sandbox el tiempo avanza
    /// aceleradamente (MinutesPerRealSecond configurable, prompt §23).
    /// </summary>
    public class GameCalendarSystem
    {
        public WorldDate CurrentDate { get; private set; }
        public float MinutesPerRealSecond { get; private set; }

        public event Action<WorldDate> OnDateChanged;
        public event Action<int> OnHourChanged;

        private float minuteAccumulator;
        private int lastHour;

        public void Initialize(WorldDate startDate, float minutesPerRealSecond)
        {
            CurrentDate = startDate;
            MinutesPerRealSecond = minutesPerRealSecond;
            lastHour = startDate.hour;
            minuteAccumulator = 0f;
        }

        /// <summary>Debe llamarse una vez por frame (ver GameCalendarDriver).</summary>
        public void Tick(float realDeltaSeconds)
        {
            minuteAccumulator += realDeltaSeconds * MinutesPerRealSecond;
            if (minuteAccumulator < 1f) return;

            int wholeMinutes = Mathf.FloorToInt(minuteAccumulator);
            minuteAccumulator -= wholeMinutes;
            AdvanceMinutes(wholeMinutes);
        }

        /// <summary>Avance explícito de tiempo (viaje, descanso). Ver 05_TIME_SYSTEM.md §2.</summary>
        public void AdvanceMinutes(int minutes)
        {
            if (minutes <= 0) return;

            CurrentDate = CurrentDate.AddMinutes(minutes);
            OnDateChanged?.Invoke(CurrentDate);

            if (CurrentDate.hour != lastHour)
            {
                lastHour = CurrentDate.hour;
                OnHourChanged?.Invoke(lastHour);
            }
        }

        public WorldDate CaptureState() => CurrentDate;

        public void RestoreState(WorldDate date)
        {
            CurrentDate = date;
            lastHour = date.hour;
            minuteAccumulator = 0f;
            OnDateChanged?.Invoke(CurrentDate);
        }
    }
}

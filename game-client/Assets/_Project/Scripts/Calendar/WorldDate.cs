using System;

namespace LegadoPeru.Calendar
{
    /// <summary>
    /// Fecha del calendario de juego (ver 05_TIME_SYSTEM.md). Deliberadamente independiente
    /// de DateTime real. Prototipo de Fase 1: no considera años bisiestos (limitación conocida,
    /// documentada en PHASE_01_IMPLEMENTATION.md; sin impacto narrativo en este slice de pruebas).
    /// </summary>
    [Serializable]
    public struct WorldDate : IEquatable<WorldDate>
    {
        public int year;
        public int month; // 1-12
        public int day;   // 1-31
        public int hour;  // 0-23
        public int minute; // 0-59

        private static readonly int[] DaysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public WorldDate(int year, int month, int day, int hour, int minute)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
        }

        /// <summary>1 de septiembre de 1820 — fecha técnica temporal del Sandbox (prompt §23).</summary>
        public static WorldDate Default => new WorldDate(1820, 9, 1, 8, 0);

        public WorldDate AddMinutes(int minutesToAdd)
        {
            int totalMinutes = hour * 60 + minute + minutesToAdd;
            int dayDelta = totalMinutes >= 0 ? totalMinutes / 1440 : -((-totalMinutes + 1439) / 1440);
            int remMinutes = totalMinutes - dayDelta * 1440;

            int newHour = remMinutes / 60;
            int newMinute = remMinutes % 60;

            int y = year, m = month, d = day;
            int remainingDays = dayDelta;

            while (remainingDays > 0)
            {
                int dim = DaysInMonth[m - 1];
                int room = dim - d;
                if (remainingDays <= room)
                {
                    d += remainingDays;
                    remainingDays = 0;
                }
                else
                {
                    remainingDays -= room + 1;
                    d = 1;
                    m++;
                    if (m > 12) { m = 1; y++; }
                }
            }

            while (remainingDays < 0)
            {
                if (d - 1 >= -remainingDays)
                {
                    d += remainingDays;
                    remainingDays = 0;
                }
                else
                {
                    remainingDays += d;
                    m--;
                    if (m < 1) { m = 12; y--; }
                    d = DaysInMonth[m - 1];
                }
            }

            return new WorldDate(y, m, d, newHour, newMinute);
        }

        public bool Equals(WorldDate other) =>
            year == other.year && month == other.month && day == other.day && hour == other.hour && minute == other.minute;

        public override bool Equals(object obj) => obj is WorldDate other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + year;
                hash = hash * 31 + month;
                hash = hash * 31 + day;
                hash = hash * 31 + hour;
                hash = hash * 31 + minute;
                return hash;
            }
        }

        public override string ToString() => $"{day:D2}/{month:D2}/{year} {hour:D2}:{minute:D2}";
    }
}

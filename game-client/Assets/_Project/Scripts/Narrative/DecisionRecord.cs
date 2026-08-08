using System;

namespace LegadoPeru.Narrative
{
    /// <summary>
    /// Subconjunto de prototipo del esquema completo de DecisionEvent (03_DATA_MODEL.md §4),
    /// suficiente para probar persistencia de decisiones en Fase 1 (prompt §22). IDs de
    /// prototipo: aun no son GUID reales de producción.
    /// </summary>
    [Serializable]
    public class DecisionRecord
    {
        public string decisionId;
        public string characterId;
        public string locationId;
        public string selectedChoice;

        public int gameYear;
        public int gameMonth;
        public int gameDay;
        public int gameHour;
        public int gameMinute;
    }
}

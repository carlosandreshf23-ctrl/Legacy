using System.Collections.Generic;
using LegadoPeru.Calendar;
using LegadoPeru.Core;
using LegadoPeru.DebugTools;

namespace LegadoPeru.Narrative

{
    /// <summary>
    /// Versión mínima de DecisionSystem/ConsequenceEngine (07_CONSEQUENCE_SYSTEM.md) para
    /// Fase 1: solo registra y consulta decisiones, sin evaluación de TriggerConditions
    /// todavía (eso llega en Fase 4).
    /// </summary>
    public class DecisionService
    {
        private readonly List<DecisionRecord> records = new List<DecisionRecord>();

        public IReadOnlyList<DecisionRecord> Records => records;

        public void RecordDecision(string decisionId, string characterId, string locationId, string selectedChoice)
        {
            WorldDate date = ServiceLocator.TryGet(out GameCalendarSystem calendar)
                ? calendar.CurrentDate
                : WorldDate.Default;

            records.Add(new DecisionRecord
            {
                decisionId = decisionId,
                characterId = characterId,
                locationId = locationId,
                selectedChoice = selectedChoice,
                gameYear = date.year,
                gameMonth = date.month,
                gameDay = date.day,
                gameHour = date.hour,
                gameMinute = date.minute
            });

            DebugLog.Log(DebugLog.Category.Narrative, $"Decision recorded: {decisionId} = {selectedChoice}");
        }

        public bool TryGetChoice(string decisionId, out string choice)
        {
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (records[i].decisionId != decisionId) continue;
                choice = records[i].selectedChoice;
                return true;
            }

            choice = null;
            return false;
        }

        public List<DecisionRecord> CaptureState() => new List<DecisionRecord>(records);

        public void RestoreState(List<DecisionRecord> data)
        {
            records.Clear();
            if (data != null) records.AddRange(data);
        }
    }
}

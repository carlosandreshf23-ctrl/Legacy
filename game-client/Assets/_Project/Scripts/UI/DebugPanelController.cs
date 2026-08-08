using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Core;
using LegadoPeru.Narrative;
using LegadoPeru.World;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>
    /// Development Mode panel (prompt Fase 1 §25 y Fase 2 §42): CurrentDate, CurrentLocation,
    /// CharacterID, FamilyID, FPS, DecisionCount, VisitedLocations. ActiveQuest se deja
    /// preparado ("—") hasta que exista QuestGraph (07_CONSEQUENCE_SYSTEM.md, fase posterior).
    /// Nunca debe incluirse en una build comercial (prompt §44).
    /// </summary>
    public class DebugPanelController : MonoBehaviour
    {
        public GameObject panelRoot;
        public Text contentText;

        private float fpsAccum;
        private int fpsFrames;
        private float fps;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1)) Toggle();

            fpsAccum += Time.unscaledDeltaTime;
            fpsFrames++;
            if (fpsAccum >= 0.5f)
            {
                fps = fpsFrames / fpsAccum;
                fpsAccum = 0f;
                fpsFrames = 0;
            }

            if (panelRoot == null || !panelRoot.activeSelf || contentText == null) return;

            var calendar = ServiceLocator.TryGet(out GameCalendarSystem cal) ? cal.CurrentDate.ToString() : "-";
            var locationId = ServiceLocator.TryGet(out LocationSystem loc) ? loc.CurrentLocationId : "-";

            string characterId = "-", familyId = "-", position = "-";
            if (ServiceLocator.TryGet(out PlayerCharacterController player))
            {
                characterId = player.characterId;
                familyId = player.familyId;
                position = player.transform.position.ToString("F1");
            }

            int decisionCount = ServiceLocator.TryGet(out DecisionService decisions) ? decisions.Records.Count : 0;
            int visitedCount = 0;
            if (ServiceLocator.TryGet(out DiscoverySystem discovery))
            {
                foreach (var entry in discovery.CaptureState())
                    if (entry.state == DiscoveryState.Visited) visitedCount++;
            }

            contentText.text =
                $"{BuildInfo.DisplayString}\n" +
                $"GameDate: {calendar}\n" +
                $"FPS: {fps:F0}\n" +
                $"Position: {position}\n" +
                $"Location: {locationId}\n" +
                $"CharacterID: {characterId}\n" +
                $"FamilyID: {familyId}\n" +
                $"ActiveQuest: — (QuestGraph pendiente)\n" +
                $"DecisionCount: {decisionCount}\n" +
                $"VisitedLocations: {visitedCount}";
        }

        public void Toggle()
        {
            if (panelRoot != null) panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }
}

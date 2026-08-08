using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Core;
using LegadoPeru.World;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>
    /// Panel de debug activable/desactivable (prompt §25): GameDate, GameTime, posición,
    /// FPS, CurrentLocationId. No es UI final de jugador.
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
            var position = ServiceLocator.TryGet(out PlayerCharacterController player)
                ? player.transform.position.ToString("F1")
                : "-";

            contentText.text =
                $"GameDate: {calendar}\n" +
                $"FPS: {fps:F0}\n" +
                $"Position: {position}\n" +
                $"Location: {locationId}";
        }

        public void Toggle()
        {
            if (panelRoot != null) panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }
}

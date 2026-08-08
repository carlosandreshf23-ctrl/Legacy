using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.Calendar
{
    /// <summary>
    /// MVP de ciclo día/noche (prompt §24). Rota la luz direccional y ajusta color/intensidad
    /// según la hora de GameCalendarSystem. No busca simulación astronómica exacta; sustituible
    /// más adelante por datos geográficos reales sin cambiar el contrato con GameCalendarSystem.
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Color nightColor = new Color(0.15f, 0.18f, 0.32f);
        [SerializeField] private Color dayColor = new Color(1f, 0.96f, 0.88f);
        [SerializeField] private float nightIntensity = 0.05f;
        [SerializeField] private float dayIntensity = 1.15f;

        public void SetSun(Light sunLight) => sun = sunLight;

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out GameCalendarSystem calendar))
            {
                calendar.OnDateChanged += HandleDateChanged;
                HandleDateChanged(calendar.CurrentDate);
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet(out GameCalendarSystem calendar))
                calendar.OnDateChanged -= HandleDateChanged;
        }

        private void HandleDateChanged(WorldDate date)
        {
            if (sun == null) return;

            float dayFraction = (date.hour + date.minute / 60f) / 24f;
            float sunAngle = dayFraction * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            // Curva simple: máximo de luz al mediodía (0.5), mínimo a medianoche (0.0).
            float t = Mathf.Clamp01(Mathf.Sin((dayFraction - 0.25f) * Mathf.PI * 2f) * 0.5f + 0.5f);
            sun.color = Color.Lerp(nightColor, dayColor, t);
            sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(0.2f, 1f, t);
        }
    }
}

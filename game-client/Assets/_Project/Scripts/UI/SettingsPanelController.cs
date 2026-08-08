using LegadoPeru.Core;
using LegadoPeru.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>Panel de configuración MVP (prompt §37): volumen, sensibilidad de cámara, calidad gráfica.</summary>
    public class SettingsPanelController : MonoBehaviour
    {
        public GameObject panelRoot;
        public Slider volumeSlider;
        public Slider sensitivitySlider;
        public Dropdown qualityDropdown;
        public Button closeButton;

        private void Awake()
        {
            if (!ServiceLocator.TryGet(out SettingsService settings)) return;

            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.value = settings.Data.masterVolume;
                volumeSlider.onValueChanged.AddListener(v =>
                {
                    settings.Data.masterVolume = v;
                    settings.Apply();
                    settings.Save();
                });
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = 0.2f;
                sensitivitySlider.maxValue = 3f;
                sensitivitySlider.value = settings.Data.cameraSensitivity;
                sensitivitySlider.onValueChanged.AddListener(v =>
                {
                    settings.Data.cameraSensitivity = v;
                    settings.Save();
                });
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Low", "Medium", "High" });
                qualityDropdown.value = Mathf.Clamp(settings.Data.qualityLevel, 0, 2);
                qualityDropdown.onValueChanged.AddListener(v =>
                {
                    settings.Data.qualityLevel = v;
                    settings.Apply();
                    settings.Save();
                });
            }

            closeButton?.onClick.AddListener(() => panelRoot?.SetActive(false));
        }
    }
}

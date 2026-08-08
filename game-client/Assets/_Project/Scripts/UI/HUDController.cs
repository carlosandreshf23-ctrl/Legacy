using LegadoPeru.Calendar;
using LegadoPeru.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>HUD principal MVP (prompt §35): prompt de interacción, stamina, fecha/hora, botones de inventario/pausa.</summary>
    public class HUDController : MonoBehaviour
    {
        public Text interactionPromptText;
        public Slider staminaSlider;
        public Text dateTimeText;
        public Button inventoryButton;
        public Button pauseButton;
        public GameObject inventoryPanel;
        public GameObject pausePanel;

        private void OnEnable()
        {
            if (interactionPromptText != null) interactionPromptText.text = string.Empty;

            EventBus.Subscribe<InteractionPromptChangedEvent>(OnPromptChanged);
            EventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);

            if (ServiceLocator.TryGet(out GameCalendarSystem calendar))
            {
                calendar.OnDateChanged += OnDateChanged;
                OnDateChanged(calendar.CurrentDate);
            }

            if (inventoryButton != null) inventoryButton.onClick.AddListener(ToggleInventory);
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractionPromptChangedEvent>(OnPromptChanged);
            EventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);

            if (ServiceLocator.TryGet(out GameCalendarSystem calendar))
                calendar.OnDateChanged -= OnDateChanged;
        }

        private void OnPromptChanged(InteractionPromptChangedEvent e)
        {
            if (interactionPromptText != null)
                interactionPromptText.text = string.IsNullOrEmpty(e.Prompt) ? string.Empty : e.Prompt;
        }

        private void OnStaminaChanged(StaminaChangedEvent e)
        {
            if (staminaSlider == null) return;
            staminaSlider.maxValue = e.Max;
            staminaSlider.value = e.Current;
        }

        private void OnDateChanged(WorldDate date)
        {
            if (dateTimeText != null) dateTimeText.text = date.ToString();
        }

        private void ToggleInventory()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }

        private void TogglePause()
        {
            if (pausePanel != null) pausePanel.SetActive(!pausePanel.activeSelf);
        }
    }
}

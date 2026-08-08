using LegadoPeru.Core;
using LegadoPeru.InputSystem;
using LegadoPeru.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>Menú de pausa: reanudar, guardar manualmente, ajustes, volver al menú principal.</summary>
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject panelRoot;
        public GameObject settingsPanel;
        public Button resumeButton;
        public Button saveButton;
        public Button settingsButton;
        public Button quitToMenuButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            resumeButton?.onClick.AddListener(() => panelRoot.SetActive(false));
            saveButton?.onClick.AddListener(() => ServiceLocator.Get<SaveSystem>().SaveToSlot("manual_save"));
            settingsButton?.onClick.AddListener(() => settingsPanel?.SetActive(true));
            quitToMenuButton?.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        }

        private void Update()
        {
            if (ServiceLocator.TryGet(out GameInput input) && input.PausePressed && panelRoot != null)
                panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }
}

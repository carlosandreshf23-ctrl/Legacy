using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>Menú principal MVP (prompt §36): Nueva partida / Continuar / Cargar / Configuración / Salir.</summary>
    public class MainMenuController : MonoBehaviour
    {
        public Button newGameButton;
        public Button continueButton;
        public Button loadGameButton;
        public Button settingsButton;
        public Button quitButton;
        public GameObject settingsPanel;

        private const string SandboxSceneName = "Sandbox_1820_Prototype";

        private void Awake()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();

            bool hasAnySave = saveSystem.HasSave("autosave") || saveSystem.HasSave("manual_save");
            if (continueButton != null) continueButton.interactable = hasAnySave;
            if (loadGameButton != null) loadGameButton.interactable = hasAnySave;

            newGameButton?.onClick.AddListener(StartNewGame);
            continueButton?.onClick.AddListener(ContinueGame);
            loadGameButton?.onClick.AddListener(() => LoadSlot("manual_save"));
            settingsButton?.onClick.AddListener(() => settingsPanel?.SetActive(true));
            quitButton?.onClick.AddListener(() => Application.Quit());
        }

        private void StartNewGame()
        {
            PendingLoadRequest.Clear();
            SceneManager.LoadScene(SandboxSceneName);
        }

        private void ContinueGame()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            string slot = saveSystem.HasSave("autosave") ? "autosave" : "manual_save";
            LoadSlot(slot);
        }

        private void LoadSlot(string slot)
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            if (saveSystem.TryLoadFromSlot(slot, out var data))
            {
                PendingLoadRequest.Set(data);
                SceneManager.LoadScene(SandboxSceneName);
            }
            else
            {
                DebugLog.Log(DebugLog.Category.Save, $"No se encontró un save válido en '{slot}'.");
            }
        }
    }
}

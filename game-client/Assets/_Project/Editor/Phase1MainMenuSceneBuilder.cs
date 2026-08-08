using LegadoPeru.Core;
using LegadoPeru.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegadoPeru.EditorTools
{
    /// <summary>Construye MainMenu.unity (prompt §36): Nueva partida / Continuar / Cargar / Configuración / Salir.</summary>
    public static class Phase1MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";

        private static Font uiFontCache;

        [MenuItem("Legado/Fase 1/3. Build Main Menu Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneSetupMode.SingleScene);

            var camGO = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            camGO.GetComponent<Camera>().backgroundColor = new Color(0.06f, 0.05f, 0.04f);
            camGO.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var title = CreateText(canvasGO.transform, "Title", "LEGADO: PERÚ", 56, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(900f, 90f));

            var subtitle = CreateText(canvasGO.transform, "Subtitle", "Fase 1 — Fundación Jugable (prototipo, sin validar históricamente)", 20, TextAnchor.MiddleCenter);
            Anchor(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(900f, 40f));

            var newGame = CreateButton(canvasGO.transform, "NewGameButton", "Nueva partida");
            Anchor(newGame.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(320f, 60f));

            var continueGame = CreateButton(canvasGO.transform, "ContinueButton", "Continuar");
            Anchor(continueGame.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(320f, 60f));

            var loadGame = CreateButton(canvasGO.transform, "LoadGameButton", "Cargar partida");
            Anchor(loadGame.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(320f, 60f));

            var settingsButton = CreateButton(canvasGO.transform, "SettingsButton", "Configuración");
            Anchor(settingsButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(320f, 60f));

            var quitButton = CreateButton(canvasGO.transform, "QuitButton", "Salir");
            Anchor(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(320f, 60f));

            var settingsPanel = BuildSettingsPanel(canvasGO.transform);

            var menuController = canvasGO.AddComponent<MainMenuController>();
            menuController.newGameButton = newGame;
            menuController.continueButton = continueGame;
            menuController.loadGameButton = loadGame;
            menuController.settingsButton = settingsButton;
            menuController.quitButton = quitButton;
            menuController.settingsPanel = settingsPanel;

            Phase1ContentSeeder.EnsureFolder(ScenePath);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[Legado] MainMenu.unity generada en " + ScenePath);
        }

        private static GameObject BuildSettingsPanel(Transform canvasTransform)
        {
            var panel = CreatePanel(canvasTransform, "SettingsPanel", new Vector2(480f, 420f));
            panel.SetActive(false);

            CreateText(panel.transform, "VolumeLabel", "Volumen", 22, TextAnchor.MiddleLeft);
            Anchor(panel.transform.Find("VolumeLabel").GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -30f), new Vector2(0f, 30f));
            var volumeSlider = CreateSlider(panel.transform, "VolumeSlider");
            Anchor(volumeSlider.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), new Vector2(0f, 24f));

            CreateText(panel.transform, "SensitivityLabel", "Sensibilidad de cámara", 22, TextAnchor.MiddleLeft);
            Anchor(panel.transform.Find("SensitivityLabel").GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(0f, 30f));
            var sensitivitySlider = CreateSlider(panel.transform, "SensitivitySlider");
            Anchor(sensitivitySlider.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -160f), new Vector2(0f, 24f));

            var closeButton = CreateButton(panel.transform, "CloseButton", "Cerrar");
            Anchor(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(160f, 50f));

            var controller = panel.AddComponent<SettingsPanelController>();
            controller.panelRoot = panel;
            controller.volumeSlider = volumeSlider;
            controller.sensitivitySlider = sensitivitySlider;
            controller.closeButton = closeButton;

            return panel;
        }

        // ----- Helpers (idénticos en espíritu a Phase1SandboxSceneBuilder; duplicados
        // deliberadamente para mantener cada builder de escena independiente y legible) -----

        private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font GetUIFont()
        {
            if (uiFontCache != null) return uiFontCache;
            uiFontCache = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFontCache == null) uiFontCache = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return uiFontCache;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = GetUIFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var text = CreateText(go.transform, "Label", label, 24, TextAnchor.MiddleCenter);
            StretchFull(text.rectTransform);

            return go.GetComponent<Button>();
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            return go;
        }

        private static Slider CreateSlider(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            StretchFull((RectTransform)bg.transform);
            bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            StretchFull((RectTransform)fillArea.transform);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            StretchFull((RectTransform)fill.transform);
            fill.GetComponent<Image>().color = new Color(0.75f, 0.55f, 0.2f, 0.95f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }
    }
}

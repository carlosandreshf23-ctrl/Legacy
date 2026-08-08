using System.Collections.Generic;
using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Dialogue;
using LegadoPeru.Interaction;
using LegadoPeru.Inventory;
using LegadoPeru.Persistence;
using LegadoPeru.UI;
using LegadoPeru.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegadoPeru.EditorTools
{
    /// <summary>
    /// Construye Sandbox_1820_Prototype.unity de forma procedural (bloques/primitivas,
    /// prompt §5-6 y §46: prioridad a gameplay sobre arte). Se ejecuta dentro del Editor
    /// para que Unity genere la serialización de escena correctamente — evita autoría
    /// manual de YAML de escena, que sería frágil sin poder validarla fuera del Editor.
    /// Ver PHASE_01_IMPLEMENTATION.md para la tabla de construcción manual alternativa.
    /// </summary>
    public static class Phase1SandboxSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox_1820_Prototype.unity";
        private const string DataRoot = "Assets/_Project/Data/Resources";

        private static Font uiFontCache;

        [MenuItem("Legado/Fase 1/2. Build Sandbox Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneSetupMode.SingleScene);

            var sun = CreateLighting();
            CreateGround();
            CreatePathAndObstacles();

            var movementConfig = Phase1ContentSeeder.CreateOrLoad<MovementConfig>(DataRoot + "/MovementConfig.asset", _ => { });

            var player = CreatePlayer(movementConfig);
            CreateCamera(player.transform);
            CreateSystemsRoot(sun);

            CreateLocationTriggers();
            var npc = CreateNPC();
            CreateSupplyBox();

            CreateUI(npc);

            var sceneControllerGO = new GameObject("_SandboxSceneController");
            sceneControllerGO.AddComponent<SandboxSceneController>();
            sceneControllerGO.AddComponent<AutosaveController>();

            Phase1ContentSeeder.EnsureFolder(ScenePath);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[Legado] Sandbox_1820_Prototype.unity generada en " + ScenePath);
        }

        // ----- Mundo (blockout) -----

        private static Light CreateLighting()
        {
            var sunGO = new GameObject("Sun");
            var sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sunGO.transform.rotation = Quaternion.Euler(50f, 170f, 0f);

            RenderSettings.sun = sun;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);

            return sun;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Blockout";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(45f, 1f, 45f); // ~450x450 m, dentro del objetivo de 300-600m (prompt §6)
            ApplyColor(ground, new Color(0.76f, 0.70f, 0.52f));
        }

        private static void CreatePathAndObstacles()
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "Path_Blockout";
            path.transform.position = new Vector3(0f, 0.02f, -20f);
            path.transform.localScale = new Vector3(6f, 0.05f, 170f);
            ApplyColor(path, new Color(0.55f, 0.48f, 0.35f));

            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle_Rock_01";
            obstacle.transform.position = new Vector3(10f, 0.75f, 10f);
            obstacle.transform.localScale = new Vector3(3f, 1.5f, 2.5f);
            ApplyColor(obstacle, new Color(0.5f, 0.5f, 0.48f));

            var slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = "Slope_Blockout";
            slope.transform.position = new Vector3(-22f, 1f, 40f);
            slope.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            slope.transform.localScale = new Vector3(12f, 0.5f, 22f);
            ApplyColor(slope, new Color(0.7f, 0.65f, 0.5f));

            var house = GameObject.CreatePrimitive(PrimitiveType.Cube);
            house.name = "House_Blockout";
            house.transform.position = new Vector3(-28f, 1.5f, -30f);
            house.transform.localScale = new Vector3(8f, 3f, 8f);
            ApplyColor(house, new Color(0.82f, 0.76f, 0.6f));
        }

        private static void CreateLocationTriggers()
        {
            var coast = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_Coast.asset");
            var path = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_Path.asset");
            var house = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_House.asset");

            CreateLocationTrigger("Coast", coast, new Vector3(0f, 2f, -110f), new Vector3(260f, 10f, 70f));
            CreateLocationTrigger("Path", path, new Vector3(0f, 2f, -10f), new Vector3(30f, 10f, 150f));
            CreateLocationTrigger("House", house, new Vector3(-28f, 2f, -30f), new Vector3(20f, 10f, 20f));
        }

        private static void CreateLocationTrigger(string label, LocationDefinition def, Vector3 center, Vector3 size)
        {
            var go = new GameObject("Location_" + label);
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            go.AddComponent<LocationTrigger>().SetLocation(def);
        }

        private static GameObject CreatePlayer(MovementConfig movementConfig)
        {
            var player = new GameObject("Player", typeof(CharacterController));
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.05f, -110f);

            var controller = player.GetComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual_Blockout";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            ApplyColor(visual, new Color(0.30f, 0.45f, 0.65f));

            player.AddComponent<CharacterMotor>().SetConfig(movementConfig);
            player.AddComponent<PlayerCharacterController>();
            player.AddComponent<FootstepPlayer>();
            player.AddComponent<InteractionSystem>();

            return player;
        }

        private static void CreateCamera(Transform target)
        {
            var camGO = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            var cam = camGO.GetComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 300f;
            camGO.transform.position = target.position + new Vector3(0f, 2f, -4.5f);

            camGO.AddComponent<ThirdPersonCameraController>().SetTarget(target);
        }

        private static void CreateSystemsRoot(Light sun)
        {
            var systemsGO = new GameObject("_Systems");
            systemsGO.AddComponent<GameCalendarDriver>();
            systemsGO.AddComponent<DayNightController>().SetSun(sun);
        }

        private static NPCController CreateNPC()
        {
            var npcGO = new GameObject("NPC_Test_01");
            npcGO.transform.position = new Vector3(6f, 1f, -20f);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual_Blockout";
            visual.transform.SetParent(npcGO.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            ApplyColor(visual, new Color(0.6f, 0.35f, 0.25f));

            var pointA = new GameObject("PatrolPoint_A").transform;
            pointA.position = npcGO.transform.position + new Vector3(4f, 0f, 0f);
            var pointB = new GameObject("PatrolPoint_B").transform;
            pointB.position = npcGO.transform.position + new Vector3(-4f, 0f, 3f);

            var npc = npcGO.AddComponent<NPCController>();
            npc.SetPersistentId("NPC_Test_01");
            npc.SetNpcId("NPC_Test_01");
            npc.SetPatrolPoints(new List<Transform> { pointA, pointB });

            var dialogue = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(DataRoot + "/Dialogue/NPC_Test_01.asset");
            npc.SetDialogue(dialogue);

            return npc;
        }

        private static void CreateSupplyBox()
        {
            var boxGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxGO.name = "SupplyBox_Prototype_001";
            boxGO.transform.position = new Vector3(-4f, 0.4f, -15f);
            boxGO.transform.localScale = new Vector3(0.8f, 0.6f, 0.6f);
            ApplyColor(boxGO, new Color(0.55f, 0.4f, 0.2f));

            var emptyOverlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
            emptyOverlay.name = "Visual_Empty_Overlay";
            emptyOverlay.transform.SetParent(boxGO.transform, false);
            emptyOverlay.transform.localScale = Vector3.one * 0.92f;
            Object.DestroyImmediate(emptyOverlay.GetComponent<Collider>());
            ApplyColor(emptyOverlay, new Color(0.25f, 0.22f, 0.18f));
            emptyOverlay.SetActive(false);

            var supplyBox = boxGO.AddComponent<SupplyBoxInteractable>();
            supplyBox.SetPersistentId("SupplyBox_Prototype_001");
            supplyBox.SetItem("ITEM_PROVISIONS", 1);
            supplyBox.SetVisuals(null, emptyOverlay);
        }

        // ----- UI -----

        private static void CreateUI(NPCController npc)
        {
            EnsureEventSystem();

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var hud = BuildHud(canvasGO.transform);
            var inventoryPanel = BuildInventoryPanel(canvasGO.transform, hud);
            var dialoguePanel = BuildDialoguePanel(canvasGO.transform);
            var settingsPanel = BuildSettingsPanel(canvasGO.transform);
            var pausePanel = BuildPausePanel(canvasGO.transform, settingsPanel);
            var debugPanel = BuildDebugPanel(canvasGO.transform);
            var mapPanel = BuildMapPanel(canvasGO.transform, npc);

            hud.inventoryPanel = inventoryPanel;
            hud.pausePanel = pausePanel;

            // Los controles táctiles se construyen siempre (activos/inactivos se decide en
            // runtime dentro de GameInput.Awake, nunca en tiempo de Editor: Application.
            // isMobilePlatform en el Editor de escritorio siempre es false).
            BuildTouchControls(canvasGO.transform);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static HUDController BuildHud(Transform canvasTransform)
        {
            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvasTransform, false);
            StretchFull((RectTransform)hudGO.transform);

            var prompt = CreateText(hudGO.transform, "InteractionPrompt", string.Empty, 30, TextAnchor.MiddleCenter);
            Anchor(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(700f, 50f));

            var staminaSlider = CreateSlider(hudGO.transform, "StaminaSlider");
            Anchor(staminaSlider.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(150f, 40f), new Vector2(220f, 22f));

            var dateTimeText = CreateText(hudGO.transform, "DateTimeText", string.Empty, 22, TextAnchor.UpperRight);
            Anchor(dateTimeText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-170f, -30f), new Vector2(320f, 36f));

            var inventoryButton = CreateButton(hudGO.transform, "InventoryButton", "Inventario");
            Anchor(inventoryButton.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(80f, 110f), new Vector2(140f, 55f));

            var pauseButton = CreateButton(hudGO.transform, "PauseButton", "Pausa");
            Anchor(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -100f), new Vector2(120f, 55f));

            var hud = hudGO.AddComponent<HUDController>();
            hud.interactionPromptText = prompt;
            hud.staminaSlider = staminaSlider;
            hud.dateTimeText = dateTimeText;
            hud.inventoryButton = inventoryButton;
            hud.pauseButton = pauseButton;

            return hud;
        }

        private static GameObject BuildInventoryPanel(Transform canvasTransform, HUDController hud)
        {
            var panel = CreatePanel(canvasTransform, "InventoryPanel", new Vector2(520f, 620f));
            panel.SetActive(false);

            CreateText(panel.transform, "Title", "Inventario", 28, TextAnchor.UpperLeft);
            Anchor(panel.transform.Find("Title").GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(0f, 40f));

            var contentParent = CreateListContent(panel.transform);

            var closeButton = CreateButton(panel.transform, "CloseButton", "Cerrar");
            Anchor(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(160f, 50f));
            closeButton.onClick.AddListener(() => panel.SetActive(false));

            var controller = panel.AddComponent<InventoryUIController>();
            controller.contentParent = contentParent;

            return panel;
        }

        private static GameObject BuildDialoguePanel(Transform canvasTransform)
        {
            var panel = CreatePanel(canvasTransform, "DialoguePanel", new Vector2(1000f, 320f));
            Anchor(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1000f, 320f));
            panel.SetActive(false);

            var speaker = CreateText(panel.transform, "Speaker", string.Empty, 26, TextAnchor.UpperLeft);
            Anchor(speaker.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(0f, 36f));

            var body = CreateText(panel.transform, "Body", string.Empty, 24, TextAnchor.UpperLeft);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            Anchor(body.rectTransform, new Vector2(0f, 0.35f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(-40f, 0f));

            var continueButton = CreateButton(panel.transform, "ContinueButton", "Continuar...");
            Anchor(continueButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-110f, 40f), new Vector2(180f, 55f));

            var choiceA = CreateButton(panel.transform, "ChoiceA", string.Empty);
            Anchor(choiceA.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(10f, 40f), new Vector2(-10f, 55f));

            var choiceB = CreateButton(panel.transform, "ChoiceB", string.Empty);
            Anchor(choiceB.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(10f, 40f), new Vector2(-10f, 55f));

            var controller = panel.AddComponent<DialogueUIController>();
            controller.panelRoot = panel;
            controller.speakerText = speaker;
            controller.bodyText = body;
            controller.continueButton = continueButton;
            controller.choiceButtonA = choiceA;
            controller.choiceButtonB = choiceB;
            controller.choiceTextA = choiceA.GetComponentInChildren<Text>();
            controller.choiceTextB = choiceB.GetComponentInChildren<Text>();

            return panel;
        }

        private static GameObject BuildPausePanel(Transform canvasTransform, GameObject settingsPanel)
        {
            var panel = CreatePanel(canvasTransform, "PausePanel", new Vector2(420f, 420f));
            panel.SetActive(false);

            var resume = CreateButton(panel.transform, "ResumeButton", "Reanudar");
            Anchor(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(260f, 55f));

            var save = CreateButton(panel.transform, "SaveButton", "Guardar partida");
            Anchor(save.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(260f, 55f));

            var settings = CreateButton(panel.transform, "SettingsButton", "Configuración");
            Anchor(settings.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(260f, 55f));

            var quit = CreateButton(panel.transform, "QuitToMenuButton", "Salir al menú");
            Anchor(quit.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -270f), new Vector2(260f, 55f));

            var controller = panel.AddComponent<PauseMenuController>();
            controller.panelRoot = panel;
            controller.settingsPanel = settingsPanel;
            controller.resumeButton = resume;
            controller.saveButton = save;
            controller.settingsButton = settings;
            controller.quitToMenuButton = quit;

            return panel;
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

            CreateText(panel.transform, "QualityLabel", "Calidad gráfica", 22, TextAnchor.MiddleLeft);
            Anchor(panel.transform.Find("QualityLabel").GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -210f), new Vector2(0f, 30f));
            var qualityDropdown = CreateDropdown(panel.transform, "QualityDropdown");
            Anchor(qualityDropdown.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -250f), new Vector2(0f, 34f));

            var closeButton = CreateButton(panel.transform, "CloseButton", "Cerrar");
            Anchor(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(160f, 50f));

            var controller = panel.AddComponent<SettingsPanelController>();
            controller.panelRoot = panel;
            controller.volumeSlider = volumeSlider;
            controller.sensitivitySlider = sensitivitySlider;
            controller.qualityDropdown = qualityDropdown;
            controller.closeButton = closeButton;

            return panel;
        }

        private static GameObject BuildDebugPanel(Transform canvasTransform)
        {
            var panel = CreatePanel(canvasTransform, "DebugPanel", new Vector2(420f, 200f));
            Anchor(panel.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(230f, -110f), new Vector2(420f, 200f));
            panel.SetActive(false);

            var content = CreateText(panel.transform, "Content", string.Empty, 20, TextAnchor.UpperLeft);
            StretchFull(content.rectTransform);

            var controller = panel.AddComponent<DebugPanelController>();
            controller.panelRoot = panel;
            controller.contentText = content;

            return panel;
        }

        private static GameObject BuildMapPanel(Transform canvasTransform, NPCController npc)
        {
            var panel = CreatePanel(canvasTransform, "MapPanel", new Vector2(420f, 420f));
            panel.SetActive(false);

            var mapArea = new GameObject("MapArea", typeof(RectTransform), typeof(Image));
            mapArea.transform.SetParent(panel.transform, false);
            var mapAreaRect = mapArea.GetComponent<RectTransform>();
            Anchor(mapAreaRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 380f));
            mapArea.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.12f, 0.9f);

            var playerMarker = CreateMarker(mapAreaRect, "PlayerMarker", Color.cyan, 14f);
            var coastMarker = CreateMarker(mapAreaRect, "CoastMarker", Color.white, 18f);
            var pathMarker = CreateMarker(mapAreaRect, "PathMarker", Color.white, 18f);
            var houseMarker = CreateMarker(mapAreaRect, "HouseMarker", Color.white, 18f);

            var controller = panel.AddComponent<MapMVPController>();
            controller.panelRoot = panel;
            controller.mapArea = mapAreaRect;
            controller.playerMarker = playerMarker;
            controller.worldOriginXZ = new Vector2(-190f, -190f);
            controller.worldSizeXZ = new Vector2(380f, 380f);

            controller.markers = new List<MapMVPController.MapMarker>
            {
                new MapMVPController.MapMarker
                {
                    location = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_Coast.asset"),
                    icon = coastMarker
                },
                new MapMVPController.MapMarker
                {
                    location = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_Path.asset"),
                    icon = pathMarker
                },
                new MapMVPController.MapMarker
                {
                    location = AssetDatabase.LoadAssetAtPath<LocationDefinition>(DataRoot + "/Locations/Prototype_House.asset"),
                    icon = houseMarker
                }
            };

            coastMarker.anchoredPosition = new Vector2(190f, 40f);
            pathMarker.anchoredPosition = new Vector2(190f, 190f);
            houseMarker.anchoredPosition = new Vector2(30f, 250f);

            return panel;
        }

        private static RectTransform CreateMarker(RectTransform parent, string name, Color color, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static void BuildTouchControls(Transform canvasTransform)
        {
            // Contenedor que GameInput activa/desactiva en runtime según UsingTouch
            // (nunca decidido en tiempo de Editor: ver comentario en GameInput.Awake).
            var touchRoot = new GameObject("TouchControls_Root", typeof(RectTransform));
            touchRoot.transform.SetParent(canvasTransform, false);
            StretchFull((RectTransform)touchRoot.transform);

            var joystickBg = new GameObject("MoveJoystick_Background", typeof(RectTransform), typeof(Image));
            joystickBg.transform.SetParent(touchRoot.transform, false);
            Anchor(joystickBg.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(160f, 200f), new Vector2(220f, 220f));
            joystickBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

            var joystickHandle = new GameObject("MoveJoystick_Handle", typeof(RectTransform), typeof(Image));
            joystickHandle.transform.SetParent(joystickBg.transform, false);
            joystickHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);
            joystickHandle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.4f);

            var joystick = joystickBg.AddComponent<LegadoPeru.InputSystem.VirtualJoystick>();
            joystick.Configure(joystickBg.GetComponent<RectTransform>(), joystickHandle.GetComponent<RectTransform>(), 90f);

            var lookPadGO = new GameObject("LookPad", typeof(RectTransform), typeof(Image));
            lookPadGO.transform.SetParent(touchRoot.transform, false);
            Anchor(lookPadGO.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 200f), new Vector2(420f, 420f));
            lookPadGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);
            var lookPad = lookPadGO.AddComponent<LegadoPeru.InputSystem.TouchLookPad>();

            var interactButton = CreateButton(touchRoot.transform, "InteractButton", "Interactuar");
            Anchor(interactButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-160f, 380f), new Vector2(180f, 70f));

            var runButton = CreateButton(touchRoot.transform, "RunButton", "Correr");
            Anchor(runButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-160f, 460f), new Vector2(180f, 70f));

            // GameInput vive fuera de TouchControls_Root: la fachada de input debe seguir
            // existiendo aunque UsingTouch sea false (PC), solo cambia qué IInputProvider usa.
            var gameInputGO = new GameObject("GameInput");
            gameInputGO.transform.SetParent(canvasTransform, false);
            var gameInput = gameInputGO.AddComponent<LegadoPeru.InputSystem.GameInput>();
            gameInput.ConfigureTouchReferences(joystick, lookPad, interactButton, runButton, touchRoot);
        }

        // ----- Helpers de UI -----

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

            var text = CreateText(go.transform, "Label", label, 22, TextAnchor.MiddleCenter);
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
            go.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.92f);
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
            slider.maxValue = 100f;
            slider.value = 100f;

            return slider;
        }

        private static Dropdown CreateDropdown(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var label = CreateText(go.transform, "Label", "Medium", 20, TextAnchor.MiddleLeft);
            Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-20f, 0f));

            var dropdown = go.GetComponent<Dropdown>();
            dropdown.captionText = label;

            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(go.transform, false);
            template.SetActive(false);
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, 150f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(template.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 28f);

            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 28f);

            var itemLabel = CreateText(item.transform, "Item Label", "Option", 18, TextAnchor.MiddleLeft);
            StretchFull(itemLabel.rectTransform);

            var toggle = item.GetComponent<Toggle>();
            toggle.targetGraphic = itemLabel;

            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            dropdown.template = templateRect;
            dropdown.itemText = itemLabel;

            return dropdown;
        }

        private static RectTransform CreateListContent(Transform parent)
        {
            var go = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(-20f, 0f);

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 4f;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null) return null;

            var mat = new Material(shader) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            return mat;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var mat = CreateMaterial(color);
            if (mat != null) renderer.sharedMaterial = mat;
        }
    }
}

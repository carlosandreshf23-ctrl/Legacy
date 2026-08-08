using System.Collections.Generic;
using LegadoPeru.Characters;
using LegadoPeru.Interaction;
using LegadoPeru.Inventory;
using LegadoPeru.Persistence;
using LegadoPeru.UI;
using LegadoPeru.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace LegadoPeru.EditorTools
{
    /// <summary>
    /// Milestone 2.1 (LOOK &amp; FEEL): construye una escena 2D cenital jugable (Grid + Tilemap
    /// por capas, cámara pixel-perfect, Mateo, un NPC, casa exterior, camino, vegetación,
    /// costa, UI mínima) a partir del pixel art generado por
    /// tools/art-pipeline/generate_milestone_2_1_assets.py. El layout de tiles reproduce
    /// exactamente el mismo grid que los mockups compuestos en Progress/Phase_02/Milestone_01,
    /// para que lo que se ve en Unity coincida con lo ya mostrado.
    ///
    /// Reemplaza a Phase1SandboxSceneBuilder (3D, retirado en el pivote de Fase 2 — prompt
    /// Fase 2 §16). Requiere haber corrido antes "Legado > Fase 2 > 0. Configure Imported Art".
    /// </summary>
    public static class Phase2LookAndFeelSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox_1820_Prototype.unity";
        private const string TilesRoot = "Assets/_Project/Art/Generated/Tiles";
        private const string CharsRoot = "Assets/_Project/Art/Generated/Characters";
        private const int PPU = 16;

        // Tamaño nativo de sprite de personaje (ver ART_BIBLE_v0.1.md — revisión de fidelidad
        // tras feedback visual: 24x36px, escala 1.5x sobre la referencia original 16x24).
        private const float CharacterSpriteHeightPx = 36f;
        private const float CharacterFeetAnchorY = CharacterSpriteHeightPx / PPU / 2f; // centro del sprite -> mitad de su alto en unidades de mundo

        private const int Cols = 20;
        private const int Rows = 14;
        private static readonly (int min, int max) HouseCols = (5, 11);
        private static readonly (int min, int max) PathCols = (7, 9);

        private static Font uiFontCache;

        [MenuItem("Legado/Fase 2/1. Build Look&Feel Scene (Milestone 2.1)")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneSetupMode.SingleScene);

            BuildTilemapLayers();
            var (mateoGO, mateoAnimator) = BuildMateo();
            BuildNpc();
            BuildCamera(mateoGO.transform);
            BuildDecorations();
            BuildUI();

            var sceneControllerGO = new GameObject("_SandboxSceneController");
            sceneControllerGO.AddComponent<SandboxSceneController>();
            sceneControllerGO.AddComponent<AutosaveController>();

            Phase1ContentSeeder.EnsureFolder(ScenePath);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[Legado] Escena 2D de Look & Feel generada en " + ScenePath +
                      " — abre MainMenu o presiona Play directamente sobre esta escena.");
        }

        // ----- Terreno: Grid + Tilemap por capas (prompt §57) -----

        private enum TileKind { Grass, Path, Sand, Water, Wall, Roof, Door }

        private static Dictionary<(int, int), TileKind> BuildLayout()
        {
            var layout = new Dictionary<(int, int), TileKind>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    bool inHouseCols = col >= HouseCols.min && col <= HouseCols.max;
                    bool inPathCols = col >= PathCols.min && col <= PathCols.max;

                    if (row <= 1) layout[(col, row)] = TileKind.Grass;
                    else if (row == 2 && inHouseCols) layout[(col, row)] = TileKind.Roof;
                    else if (row == 3 && inHouseCols) layout[(col, row)] = TileKind.Roof;
                    else if ((row == 4 || row == 5) && inHouseCols)
                        layout[(col, row)] = (row == 5 && col == 8) ? TileKind.Door : TileKind.Wall;
                    else if (row <= 6) layout[(col, row)] = TileKind.Grass;
                    else if (row >= 7 && row <= 9 && inPathCols) layout[(col, row)] = TileKind.Path;
                    else if (row >= 7 && row <= 9) layout[(col, row)] = TileKind.Grass;
                    else if (row == 10 || row == 11) layout[(col, row)] = TileKind.Sand;
                    else layout[(col, row)] = TileKind.Water;
                }
            }
            return layout;
        }

        private static void BuildTilemapLayers()
        {
            var gridGO = new GameObject("Grid", typeof(Grid));
            gridGO.GetComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);

            var ground = CreateTilemapLayer(gridGO.transform, "Tilemap_Ground", 0, solid: false);
            var water = CreateTilemapLayer(gridGO.transform, "Tilemap_Water", 0, solid: true);
            var buildings = CreateTilemapLayer(gridGO.transform, "Tilemap_Buildings", 1, solid: true);

            var tileAssets = new Dictionary<TileKind, TileBase>
            {
                [TileKind.Grass] = MakeTile(LoadSprite(TilesRoot, "tile_grass")),
                [TileKind.Path] = MakeTile(LoadSprite(TilesRoot, "tile_dirt_path")),
                [TileKind.Sand] = MakeTile(LoadSprite(TilesRoot, "tile_sand")),
                [TileKind.Water] = MakeTile(LoadSprite(TilesRoot, "tile_water_a")),
                [TileKind.Wall] = MakeTile(LoadSprite(TilesRoot, "tile_wall_adobe")),
                [TileKind.Roof] = MakeTile(LoadSprite(TilesRoot, "tile_roof_thatch")),
                [TileKind.Door] = MakeTile(LoadSprite(TilesRoot, "tile_door")),
            };

            foreach (var kvp in BuildLayout())
            {
                var (col, row) = kvp.Key;
                // Fila 0 = arriba en el layout conceptual (igual que el mockup Python); en
                // Unity +Y es "arriba", así que invertimos la fila al mapear a coordenadas de celda.
                var cell = new Vector3Int(col, Rows - 1 - row, 0);
                switch (kvp.Value)
                {
                    case TileKind.Water:
                        water.SetTile(cell, tileAssets[TileKind.Water]);
                        break;
                    case TileKind.Wall:
                    case TileKind.Roof:
                    case TileKind.Door:
                        buildings.SetTile(cell, tileAssets[kvp.Value]);
                        break;
                    default:
                        ground.SetTile(cell, tileAssets[kvp.Value]);
                        break;
                }
            }
        }

        private static Tilemap CreateTilemapLayer(Transform parent, string name, int sortingOrder, bool solid)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;

            if (solid)
            {
                var body = go.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Static;
                var tmCollider = go.AddComponent<TilemapCollider2D>();
                tmCollider.usedByComposite = true;
                go.AddComponent<CompositeCollider2D>();
            }

            return go.GetComponent<Tilemap>();
        }

        private static Tile MakeTile(Sprite sprite)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }

        private static Sprite LoadSprite(string folder, string fileName) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{fileName}.png");

        // ----- Coordenadas: convierte (col,row) del layout conceptual a posición de mundo -----

        private static Vector3 CellToWorld(float col, float row) => new Vector3(col + 0.5f, Rows - 1 - row + 0.5f, 0f);

        // ----- Personajes -----

        private static (GameObject, SpriteDirectionAnimator) BuildMateo()
        {
            var go = new GameObject("Player", typeof(Rigidbody2D), typeof(CircleCollider2D));
            go.tag = "Player";
            go.transform.position = CellToWorld(8f, 8f);

            var col = go.GetComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.offset = new Vector2(0f, -0.35f);

            var movementConfig = Phase1ContentSeeder.CreateOrLoad<MovementConfig>(
                "Assets/_Project/Data/Resources/MovementConfig.asset", _ => { });

            go.AddComponent<CharacterMotor>().SetConfig(movementConfig);
            go.AddComponent<PlayerCharacterController>();
            go.AddComponent<InteractionSystem>();

            var spriteGO = new GameObject("Sprite", typeof(SpriteRenderer));
            spriteGO.transform.SetParent(go.transform, false);
            spriteGO.transform.localPosition = new Vector3(0f, CharacterFeetAnchorY, 0f); // ancla pies a la base del tile
            var renderer = spriteGO.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;

            var animator = spriteGO.AddComponent<SpriteDirectionAnimator>();
            animator.SetFrames(LoadDirectionalFrames("Mateo_17"));

            return (go, animator);
        }

        private static void BuildNpc()
        {
            var go = new GameObject("NPC_Fisherman_Test", typeof(BoxCollider2D));
            go.transform.position = CellToWorld(6f, 9f);

            var col = go.GetComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.6f);
            col.offset = new Vector2(0f, -0.35f);

            var spriteGO = new GameObject("Sprite", typeof(SpriteRenderer));
            spriteGO.transform.SetParent(go.transform, false);
            spriteGO.transform.localPosition = new Vector3(0f, CharacterFeetAnchorY, 0f);
            spriteGO.GetComponent<SpriteRenderer>().sortingOrder = 4;
            spriteGO.GetComponent<SpriteRenderer>().sprite = LoadSprite($"{CharsRoot}/NPC_Fisherman", "NPC_Fisherman_down_0");

            // NPC de ambientación en este milestone: sin NPCController todavía (eso llega con
            // el diálogo real en Milestone 2.4). Aquí solo valida escala/legibilidad en escena.
        }

        private static SpriteDirectionAnimator.DirectionalFrames LoadDirectionalFrames(string characterName)
        {
            var frames = new SpriteDirectionAnimator.DirectionalFrames
            {
                down = LoadFrameSet(characterName, "down"),
                left = LoadFrameSet(characterName, "left"),
                up = LoadFrameSet(characterName, "up"),
            };
            return frames;
        }

        private static Sprite[] LoadFrameSet(string characterName, string direction)
        {
            var result = new Sprite[3];
            for (int i = 0; i < 3; i++)
                result[i] = LoadSprite($"{CharsRoot}/{characterName}", $"{characterName}_{direction}_{i}");
            return result;
        }

        // ----- Cámara -----

        private static void BuildCamera(Transform target)
        {
            var camGO = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            var cam = camGO.GetComponent<Camera>();
            cam.orthographic = true;
            cam.transform.position = new Vector3(target.position.x, target.position.y, -10f);

            camGO.AddComponent<PixelPerfectCamera2D>().SetTarget(target);
        }

        // ----- Decoración (props sueltos, no tilemap: árboles y postes) -----

        private static void BuildDecorations()
        {
            var treeSprite = LoadSprite(TilesRoot, "prop_tree");
            var fenceSprite = LoadSprite(TilesRoot, "prop_fence_post");

            foreach (var (col, row) in new (float, float)[] { (2, 4), (17, 3), (3, 8), (16, 7) })
                CreateProp("Tree", treeSprite, CellToWorld(col, row) + new Vector3(1f, 1f, 0f), 2);

            foreach (var (col, row) in new (float, float)[] { (4, 6), (12, 6) })
                CreateProp("FencePost", fenceSprite, CellToWorld(col, row), 2);
        }

        private static void CreateProp(string name, Sprite sprite, Vector3 position, int sortingOrder)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.position = position;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        // ----- UI mínima (prompt Milestone 2.1: "UI mínima") -----

        private static void BuildUI()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hudRect = hudGO.GetComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            var prompt = CreateText(hudGO.transform, "InteractionPrompt", string.Empty, 20);
            var promptRect = prompt.rectTransform;
            promptRect.anchorMin = new Vector2(0.5f, 0f);
            promptRect.anchorMax = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, 60f);
            promptRect.sizeDelta = new Vector2(400f, 30f);

            var hud = hudGO.AddComponent<HUDController>();
            hud.interactionPromptText = prompt;

            BuildDebugPanel(canvasGO.transform);
        }

        private static void BuildDebugPanel(Transform canvasTransform)
        {
            var panelGO = new GameObject("DebugPanel", typeof(RectTransform), typeof(Image));
            panelGO.transform.SetParent(canvasTransform, false);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(8f, -8f);
            rect.sizeDelta = new Vector2(280f, 170f);
            panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var content = CreateText(panelGO.transform, "Content", string.Empty, 14);
            content.alignment = TextAnchor.UpperLeft;
            var contentRect = content.rectTransform;
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(6f, 4f);
            contentRect.offsetMax = new Vector2(-4f, -4f);

            var debugPanel = panelGO.AddComponent<DebugPanelController>();
            debugPanel.panelRoot = panelGO;
            debugPanel.contentText = content;
        }

        private static Font GetUIFont()
        {
            if (uiFontCache != null) return uiFontCache;
            uiFontCache = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFontCache == null) uiFontCache = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return uiFontCache;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = GetUIFont();
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = content;
            return text;
        }
    }
}

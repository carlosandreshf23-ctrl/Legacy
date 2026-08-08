using UnityEditor;
using UnityEngine;

namespace LegadoPeru.EditorTools
{
    /// <summary>Registra MainMenu (índice 0) y Sandbox_1820_Prototype (índice 1) en Build Settings.</summary>
    public static class Phase1BuildSetup
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox_1820_Prototype.unity";

        [MenuItem("Legado/Fase 1/4. Add Scenes To Build Settings")]
        public static void AddScenesToBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(SandboxScenePath, true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[Legado] Build Settings actualizado: MainMenu (0), Sandbox_1820_Prototype (1).");
        }

        /// <summary>
        /// Desde Fase 2 el Sandbox se reconstruye en 2D (Phase2LookAndFeelSceneBuilder,
        /// pixel art real) en vez del blockout 3D de Fase 1 (retirado, prompt Fase 2 §16).
        /// Este método conserva el nombre/menú de Fase 1 por continuidad, pero ahora encadena
        /// el pipeline de arte + la escena 2D. Ver Legado/Fase 2 para los pasos individuales.
        /// </summary>
        [MenuItem("Legado/Fase 1/0. Build Everything (Content + Scenes + Build Settings)")]
        public static void BuildEverything()
        {
            Phase1ContentSeeder.SeedContent();
            Phase2ArtImportSetup.ConfigureImportedArt();
            Phase2LookAndFeelSceneBuilder.Build();
            Phase1MainMenuSceneBuilder.Build();
            AddScenesToBuildSettings();

            Debug.Log("[Legado] Contenido + arte + escenas 2D + build settings generados. Abre MainMenu.unity y presiona Play.");
        }
    }
}

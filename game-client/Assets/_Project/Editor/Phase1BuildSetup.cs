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

        [MenuItem("Legado/Fase 1/0. Build Everything (Content + Scenes + Build Settings)")]
        public static void BuildEverything()
        {
            Phase1ContentSeeder.SeedContent();
            Phase1SandboxSceneBuilder.Build();
            Phase1MainMenuSceneBuilder.Build();
            AddScenesToBuildSettings();

            Debug.Log("[Legado] Fase 1: contenido + escenas + build settings generados. Abre MainMenu.unity y presiona Play.");
        }
    }
}

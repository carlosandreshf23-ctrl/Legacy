using UnityEditor;
using UnityEngine;

namespace LegadoPeru.EditorTools
{
    /// <summary>
    /// Configura las texturas generadas (tools/art-pipeline/generate_milestone_2_1_assets.py)
    /// como Sprites pixel-perfect: filtro Point (sin blur), sin compresión, PixelsPerUnit=16
    /// (ver ART_BIBLE_v0.1.md §2), sin mipmaps. Debe correrse antes de construir la escena.
    /// </summary>
    public static class Phase2ArtImportSetup
    {
        private const int PixelsPerUnit = 16;

        [MenuItem("Legado/Fase 2/0. Configure Imported Art (Pixel Perfect)")]
        public static void ConfigureImportedArt()
        {
            string[] roots =
            {
                "Assets/_Project/Art/Generated/Tiles",
                "Assets/_Project/Art/Generated/Characters",
            };

            int count = 0;
            foreach (var root in roots)
            {
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = PixelsPerUnit;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.alphaIsTransparency = true;
                    importer.spritePixelsPerUnit = PixelsPerUnit;

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    count++;
                }
            }

            Debug.Log($"[Legado] {count} texturas configuradas como Sprite pixel-perfect (Point filter, PPU={PixelsPerUnit}, sin compresión).");
        }
    }
}

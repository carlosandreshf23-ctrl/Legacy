using System.Collections.Generic;
using System.IO;
using LegadoPeru.Dialogue;
using LegadoPeru.Inventory;
using LegadoPeru.World;
using UnityEditor;
using UnityEngine;

namespace LegadoPeru.EditorTools
{
    /// <summary>
    /// Genera los datos mínimos de Fase 1 como assets reales de Unity (ItemDatabase,
    /// LocationDefinition x3, DialogueDefinition de NPC_Test_01). Se ejecuta dentro del
    /// Editor para que Unity asigne los GUID de forma correcta — evita autoría manual de
    /// archivos .asset, que sería frágil sin poder validarla fuera del Editor.
    /// </summary>
    public static class Phase1ContentSeeder
    {
        private const string ResourcesRoot = "Assets/_Project/Data/Resources";

        [MenuItem("Legado/Fase 1/1. Seed Content Data")]
        public static void SeedContent()
        {
            var provisions = CreateOrLoad<ItemDefinition>(ResourcesRoot + "/ITEM_PROVISIONS.asset", def =>
            {
                def.itemId = "ITEM_PROVISIONS";
                def.displayName = "Provisiones";
                def.description = "Un pequeño surtido de alimentos y agua para el camino.";
                def.category = ItemCategory.Provisions;
                def.stackable = true;
                def.maxStack = 20;
                def.value = 1;
            });

            var database = CreateOrLoad<ItemDatabase>(ResourcesRoot + "/ItemDatabase.asset", _ => { });
            if (!database.items.Contains(provisions))
                database.items.Add(provisions);
            EditorUtility.SetDirty(database);

            CreateOrLoad<LocationDefinition>(ResourcesRoot + "/Locations/Prototype_Coast.asset", def =>
            {
                def.locationId = "Prototype_Coast";
                def.displayName = "Costa (prototipo)";
                def.regionId = "REGION_PROTOTYPE_SANDBOX";
            });

            CreateOrLoad<LocationDefinition>(ResourcesRoot + "/Locations/Prototype_Path.asset", def =>
            {
                def.locationId = "Prototype_Path";
                def.displayName = "Camino (prototipo)";
                def.regionId = "REGION_PROTOTYPE_SANDBOX";
            });

            CreateOrLoad<LocationDefinition>(ResourcesRoot + "/Locations/Prototype_House.asset", def =>
            {
                def.locationId = "Prototype_House";
                def.displayName = "Casa (prototipo)";
                def.regionId = "REGION_PROTOTYPE_SANDBOX";
            });

            CreateOrLoad<DialogueDefinition>(ResourcesRoot + "/Dialogue/NPC_Test_01.asset", def =>
            {
                def.dialogueId = "NPC_Test_01_Dialogue";
                def.startNodeId = "greet";
                def.nodes = BuildTestDialogueNodes();
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Legado] Contenido de Fase 1 sembrado: ItemDatabase (1 item), 3 LocationDefinition, 1 DialogueDefinition.");
        }

        private static List<DialogueNode> BuildTestDialogueNodes()
        {
            return new List<DialogueNode>
            {
                new DialogueNode
                {
                    nodeId = "greet",
                    speakerName = "Viajero",
                    text = "¿Vienes del camino de la costa?",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { choiceText = "Sí", nextNodeId = "greet_yes" },
                        new DialogueChoice { choiceText = "No", nextNodeId = "greet_no" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "greet_yes",
                    speakerName = "Viajero",
                    text = "Entonces conoces el camino. Ten cuidado, dicen que ronda gente por ahí.",
                    nextNodeId = "bag_question"
                },
                new DialogueNode
                {
                    nodeId = "greet_no",
                    speakerName = "Viajero",
                    text = "Ah, entonces esto te será nuevo. Bienvenido.",
                    nextNodeId = "bag_question"
                },
                new DialogueNode
                {
                    nodeId = "bag_question",
                    speakerName = "Viajero",
                    text = "Oye... perdí una bolsa con provisiones cerca del camino esta mañana. ¿La encontraste tú?",
                    decisionId = "TestDecision_Honesty",
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { choiceText = "Sí, la tengo yo", nextNodeId = "end_honest", decisionChoiceValue = "TRUE" },
                        new DialogueChoice { choiceText = "No, no la he visto", nextNodeId = "end_dishonest", decisionChoiceValue = "FALSE" }
                    }
                },
                new DialogueNode
                {
                    nodeId = "end_honest",
                    speakerName = "Viajero",
                    text = "Te lo agradezco. No todos serían tan honestos.",
                    nextNodeId = ""
                },
                new DialogueNode
                {
                    nodeId = "end_dishonest",
                    speakerName = "Viajero",
                    text = "Vaya... bueno, si la ves, avísame.",
                    nextNodeId = ""
                }
            };
        }

        internal static T CreateOrLoad<T>(string path, System.Action<T> configure) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                configure(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var instance = ScriptableObject.CreateInstance<T>();
            configure(instance);
            EnsureFolder(path);
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        internal static void EnsureFolder(string assetPath)
        {
            var folder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;

            var parts = folder.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}

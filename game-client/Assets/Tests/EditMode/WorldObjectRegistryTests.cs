using LegadoPeru.World;
using NUnit.Framework;
using UnityEngine;

namespace LegadoPeru.Tests
{
    /// <summary>Doble de prueba mínimo: expone un flag de collected como estado string, igual que SupplyBoxInteractable.</summary>
    internal class TestPersistentBox : PersistentWorldObject
    {
        public bool Collected;

        public override string CaptureState() => Collected ? "Collected" : "NotCollected";

        public override void RestoreState(string state) => Collected = state == "Collected";
    }

    /// <summary>Test 3 del prompt §41: un objeto recogido permanece recogido tras Save/Load.</summary>
    public class WorldObjectRegistryTests
    {
        [Test]
        public void CaptureAndRestore_KeepsCollectedBoxEmptyAfterReload()
        {
            var registry = new WorldObjectRegistry();

            var go = new GameObject("SupplyBox_Prototype_001");
            var box = go.AddComponent<TestPersistentBox>();
            box.SetPersistentId("SupplyBox_Prototype_001");
            box.Collected = true;
            registry.Register(box);

            var captured = registry.CaptureAll();

            // Simula una nueva sesión: registro nuevo, objeto nuevo "sin recoger" en su estado inicial.
            var freshRegistry = new WorldObjectRegistry();
            var freshGo = new GameObject("SupplyBox_Prototype_001");
            var freshBox = freshGo.AddComponent<TestPersistentBox>();
            freshBox.SetPersistentId("SupplyBox_Prototype_001");
            freshBox.Collected = false;
            freshRegistry.Register(freshBox);

            freshRegistry.RestoreAll(captured);

            Assert.IsTrue(freshBox.Collected, "La caja debe seguir vacía (Collected=true) tras recargar el save.");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(freshGo);
        }

        [Test]
        public void RestoreAll_IgnoresEntriesWithNoMatchingRegisteredObject()
        {
            var registry = new WorldObjectRegistry();
            var go = new GameObject("Unrelated");
            var box = go.AddComponent<TestPersistentBox>();
            box.SetPersistentId("Unrelated");
            registry.Register(box);

            var orphanEntry = new System.Collections.Generic.List<WorldObjectStateDto>
            {
                new WorldObjectStateDto { persistentId = "DoesNotExist", state = "Collected" }
            };

            Assert.DoesNotThrow(() => registry.RestoreAll(orphanEntry));

            Object.DestroyImmediate(go);
        }
    }
}

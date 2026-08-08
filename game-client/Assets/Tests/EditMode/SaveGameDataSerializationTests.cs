using System.Collections.Generic;
using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Inventory;
using LegadoPeru.Narrative;
using LegadoPeru.Persistence;
using LegadoPeru.World;
using NUnit.Framework;
using UnityEngine;

namespace LegadoPeru.Tests
{
    /// <summary>
    /// Verifica que el DTO raíz de guardado sobrevive un ciclo completo JsonUtility.ToJson/FromJson
    /// (prompt §32: serialización sin referencias directas a GameObject, solo IDs y datos planos).
    /// </summary>
    public class SaveGameDataSerializationTests
    {
        [Test]
        public void ToJsonFromJson_RoundTripsAllFields()
        {
            var data = new SaveGameDataV1
            {
                saveVersion = 1,
                savedAtIso = "2026-08-08T00:00:00Z",
                player = new PlayerStateDto { posX = 1f, posY = 2f, posZ = 3f, facing = "left" },
                worldDate = new WorldDate(1820, 9, 1, 10, 30),
                currentLocationId = "Prototype_Path",
                inventory = new List<ItemStack> { new ItemStack { itemId = "ITEM_PROVISIONS", quantity = 2 } },
                decisions = new List<DecisionRecord>
                {
                    new DecisionRecord
                    {
                        decisionId = "TestDecision_Honesty",
                        characterId = "SAL_MATEO_001",
                        locationId = "Prototype_Path",
                        selectedChoice = "TRUE",
                        gameYear = 1820, gameMonth = 9, gameDay = 1, gameHour = 10, gameMinute = 30
                    }
                },
                worldObjects = new List<WorldObjectStateDto>
                {
                    new WorldObjectStateDto { persistentId = "SupplyBox_Prototype_001", state = "Collected" }
                },
                discovery = new List<DiscoveryEntryDto>
                {
                    new DiscoveryEntryDto { locationId = "Prototype_Coast", state = DiscoveryState.Visited }
                }
            };

            string json = JsonUtility.ToJson(data);
            var restored = JsonUtility.FromJson<SaveGameDataV1>(json);

            Assert.AreEqual(1, restored.saveVersion);
            Assert.AreEqual(1f, restored.player.posX);
            Assert.AreEqual(1820, restored.worldDate.year);
            Assert.AreEqual("Prototype_Path", restored.currentLocationId);
            Assert.AreEqual(1, restored.inventory.Count);
            Assert.AreEqual("ITEM_PROVISIONS", restored.inventory[0].itemId);
            Assert.AreEqual(1, restored.decisions.Count);
            Assert.AreEqual("TRUE", restored.decisions[0].selectedChoice);
            Assert.AreEqual(1, restored.worldObjects.Count);
            Assert.AreEqual("Collected", restored.worldObjects[0].state);
            Assert.AreEqual(1, restored.discovery.Count);
            Assert.AreEqual(DiscoveryState.Visited, restored.discovery[0].state);
        }
    }
}

using System.Collections.Generic;
using LegadoPeru.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace LegadoPeru.Tests
{
    /// <summary>Test 2 del prompt §41: Save y Load conserva el inventario.</summary>
    public class InventoryModelTests
    {
        private ItemDatabase database;

        [SetUp]
        public void SetUp()
        {
            database = ScriptableObject.CreateInstance<ItemDatabase>();
            var provisions = ScriptableObject.CreateInstance<ItemDefinition>();
            provisions.itemId = "ITEM_PROVISIONS";
            provisions.displayName = "Provisiones";
            provisions.stackable = true;
            database.items = new List<ItemDefinition> { provisions };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(database);
        }

        [Test]
        public void AddItem_StacksSameItemInsteadOfDuplicating()
        {
            var inventory = new InventoryModel(database);
            inventory.AddItem("ITEM_PROVISIONS", 1);
            inventory.AddItem("ITEM_PROVISIONS", 2);

            Assert.AreEqual(1, inventory.Stacks.Count);
            Assert.AreEqual(3, inventory.GetQuantity("ITEM_PROVISIONS"));
        }

        [Test]
        public void CaptureAndRestore_PreservesStacksAcrossInstances()
        {
            var original = new InventoryModel(database);
            original.AddItem("ITEM_PROVISIONS", 5);

            var captured = original.CaptureState();

            var restored = new InventoryModel(database);
            restored.RestoreState(captured);

            Assert.AreEqual(5, restored.GetQuantity("ITEM_PROVISIONS"));
        }

        [Test]
        public void RemoveItem_FailsWhenNotEnoughQuantity()
        {
            var inventory = new InventoryModel(database);
            inventory.AddItem("ITEM_PROVISIONS", 1);

            bool removed = inventory.RemoveItem("ITEM_PROVISIONS", 5);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, inventory.GetQuantity("ITEM_PROVISIONS"));
        }
    }
}

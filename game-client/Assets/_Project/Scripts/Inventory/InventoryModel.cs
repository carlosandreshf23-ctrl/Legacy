using System;
using System.Collections.Generic;
using LegadoPeru.DebugTools;

namespace LegadoPeru.Inventory
{
    /// <summary>Inventario MVP (prompt §16): lista simple de ItemStack, sin peso/equipamiento/crafting.</summary>
    public class InventoryModel
    {
        private readonly ItemDatabase database;
        private readonly List<ItemStack> stacks = new List<ItemStack>();

        public IReadOnlyList<ItemStack> Stacks => stacks;
        public event Action OnChanged;

        public InventoryModel(ItemDatabase database)
        {
            this.database = database;
        }

        public void AddItem(string itemId, int quantity)
        {
            if (quantity <= 0) return;

            var definition = database != null ? database.GetById(itemId) : null;
            bool stackable = definition == null || definition.stackable;

            if (stackable)
            {
                var existing = stacks.Find(s => s.itemId == itemId);
                if (existing != null)
                {
                    existing.quantity += quantity;
                    OnChanged?.Invoke();
                    DebugLog.Log(DebugLog.Category.World, $"Item stacked: {itemId} +{quantity} (total {existing.quantity})");
                    return;
                }
            }

            stacks.Add(new ItemStack { itemId = itemId, quantity = quantity });
            OnChanged?.Invoke();
            DebugLog.Log(DebugLog.Category.World, $"Item added: {itemId} x{quantity}");
        }

        public bool RemoveItem(string itemId, int quantity)
        {
            var existing = stacks.Find(s => s.itemId == itemId);
            if (existing == null || existing.quantity < quantity) return false;

            existing.quantity -= quantity;
            if (existing.quantity <= 0) stacks.Remove(existing);

            OnChanged?.Invoke();
            return true;
        }

        public int GetQuantity(string itemId)
        {
            var existing = stacks.Find(s => s.itemId == itemId);
            return existing?.quantity ?? 0;
        }

        public ItemDefinition ResolveDefinition(string itemId) => database != null ? database.GetById(itemId) : null;

        public List<ItemStack> CaptureState()
        {
            var copy = new List<ItemStack>();
            foreach (var stack in stacks)
                copy.Add(new ItemStack { itemId = stack.itemId, quantity = stack.quantity });
            return copy;
        }

        public void RestoreState(List<ItemStack> data)
        {
            stacks.Clear();
            if (data != null) stacks.AddRange(data);
            OnChanged?.Invoke();
        }
    }
}

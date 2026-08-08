using System.Collections.Generic;
using UnityEngine;

namespace LegadoPeru.Inventory
{
    /// <summary>Catálogo de todas las ItemDefinition del proyecto, cargado vía Resources.Load en GameBootstrap.</summary>
    [CreateAssetMenu(menuName = "Legado/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemDefinition> items = new List<ItemDefinition>();

        public ItemDefinition GetById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            return items.Find(i => i != null && i.itemId == itemId);
        }
    }
}

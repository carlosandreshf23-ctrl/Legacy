using System;

namespace LegadoPeru.Inventory
{
    /// <summary>Instancia de inventario: solo ID + cantidad (nunca la definición completa), ver 03_DATA_MODEL.md.</summary>
    [Serializable]
    public class ItemStack
    {
        public string itemId;
        public int quantity;
    }
}

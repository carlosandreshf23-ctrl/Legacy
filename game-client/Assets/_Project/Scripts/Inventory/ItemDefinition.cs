using UnityEngine;

namespace LegadoPeru.Inventory
{
    public enum ItemCategory
    {
        Provisions,
        Document,
        Tool,
        Misc
    }

    /// <summary>
    /// Objetos definidos por datos (prompt §17): nunca se crean clases individuales como
    /// CornItem.cs/LetterItem.cs por cada objeto.
    /// </summary>
    [CreateAssetMenu(menuName = "Legado/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        [TextArea] public string description;
        public ItemCategory category;
        public bool stackable = true;
        public int maxStack = 20;
        public int value;
        public Sprite icon;
    }
}

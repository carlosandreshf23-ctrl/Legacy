using LegadoPeru.Core;
using LegadoPeru.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>
    /// Lista simple de inventario (prompt §16), construida en runtime porque el número de
    /// filas depende del contenido de InventoryModel. Sin peso/equipamiento/durabilidad.
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        public RectTransform contentParent;

        private Font uiFont;

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out InventoryModel inventory))
            {
                inventory.OnChanged += Refresh;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet(out InventoryModel inventory))
                inventory.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            if (contentParent == null) return;

            for (int i = contentParent.childCount - 1; i >= 0; i--)
                Destroy(contentParent.GetChild(i).gameObject);

            var inventory = ServiceLocator.Get<InventoryModel>();
            if (inventory.Stacks.Count == 0)
            {
                CreateRow("(Inventario vacío)");
                return;
            }

            foreach (var stack in inventory.Stacks)
            {
                var definition = inventory.ResolveDefinition(stack.itemId);
                string label = definition != null
                    ? $"{definition.displayName} x{stack.quantity}"
                    : $"{stack.itemId} x{stack.quantity}";
                CreateRow(label);
            }
        }

        private void CreateRow(string label)
        {
            var row = new GameObject("ItemRow", typeof(RectTransform));
            row.transform.SetParent(contentParent, false);

            var text = row.AddComponent<Text>();
            text.font = uiFont;
            text.fontSize = 24;
            text.color = Color.white;
            text.text = label;
            text.alignment = TextAnchor.MiddleLeft;

            var layout = row.AddComponent<LayoutElement>();
            layout.preferredHeight = 34;
        }
    }
}

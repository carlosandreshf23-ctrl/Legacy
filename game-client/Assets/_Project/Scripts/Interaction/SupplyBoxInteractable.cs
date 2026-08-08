using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.Inventory;
using LegadoPeru.World;
using UnityEngine;

namespace LegadoPeru.Interaction
{
    /// <summary>
    /// Primer objeto interactivo (prompt §15): caja de suministros. Al recogerse cambia de
    /// estado y ese estado se persiste vía PersistentWorldObject/WorldObjectRegistry.
    /// </summary>
    public class SupplyBoxInteractable : PersistentWorldObject, IInteractable
    {
        [SerializeField] private string itemId = "ITEM_PROVISIONS";
        [SerializeField] private int quantity = 1;
        [SerializeField] private GameObject visualFull;
        [SerializeField] private GameObject visualEmpty;

        private bool collected;

        public void SetItem(string id, int qty) { itemId = id; quantity = qty; }
        public void SetVisuals(GameObject full, GameObject empty) { visualFull = full; visualEmpty = empty; }

        public string InteractionPrompt => collected ? null : "Recoger provisiones";
        public bool CanInteract => !collected;

        protected override void Awake()
        {
            base.Awake();
            UpdateVisual();
        }

        public void Interact(GameObject interactor)
        {
            if (collected) return;

            ServiceLocator.Get<InventoryModel>().AddItem(itemId, quantity);
            collected = true;
            UpdateVisual();

            DebugLog.Log(DebugLog.Category.Interaction, $"[{PersistentId}] Provisiones recogidas.");
        }

        private void UpdateVisual()
        {
            if (visualFull != null) visualFull.SetActive(!collected);
            if (visualEmpty != null) visualEmpty.SetActive(collected);
        }

        public override string CaptureState() => collected ? "Collected" : "NotCollected";

        public override void RestoreState(string state)
        {
            collected = state == "Collected";
            UpdateVisual();
        }
    }
}

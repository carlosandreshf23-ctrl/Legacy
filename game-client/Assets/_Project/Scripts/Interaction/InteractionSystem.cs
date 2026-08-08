using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.InputSystem;
using UnityEngine;

namespace LegadoPeru.Interaction
{
    /// <summary>
    /// Detecta el IInteractable más cercano disponible (distancia + disponibilidad) y
    /// notifica al HUD vía EventBus (prompt §13). No contiene lógica específica de ningún
    /// objeto concreto: eso vive en cada implementación de IInteractable.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private LayerMask interactableMask = ~0;

        private IInteractable current;

        private void Update()
        {
            if (!ServiceLocator.TryGet(out GameInput input)) return;

            IInteractable best = FindBestInteractable();
            if (!ReferenceEquals(best, current))
            {
                current = best;
                EventBus.Publish(new InteractionPromptChangedEvent(current?.InteractionPrompt));
            }

            if (current != null && current.CanInteract && input.InteractPressed)
            {
                DebugLog.Log(DebugLog.Category.Interaction, $"Interacting via {gameObject.name}");
                current.Interact(gameObject);
            }
        }

        private IInteractable FindBestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableMask);
            IInteractable nearest = null;
            float nearestSqrDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;

                float sqrDist = (hit.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = interactable;
                }
            }

            return nearest;
        }
    }
}

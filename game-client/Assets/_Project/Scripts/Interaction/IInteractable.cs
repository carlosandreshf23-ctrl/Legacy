using UnityEngine;

namespace LegadoPeru.Interaction
{
    /// <summary>
    /// Interfaz genérica de interacción (prompt §13-14). Ningún objeto de mundo específico
    /// se referencia desde PlayerController/InteractionSystem: todos pasan por este contrato.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Texto contextual mostrado en el HUD (ej. "Recoger", "Hablar"). Null/"" oculta el prompt.</summary>
        string InteractionPrompt { get; }

        bool CanInteract { get; }

        void Interact(GameObject interactor);
    }
}

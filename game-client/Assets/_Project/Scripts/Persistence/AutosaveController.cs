using LegadoPeru.Core;
using LegadoPeru.Dialogue;
using LegadoPeru.World;
using UnityEngine;

namespace LegadoPeru.Persistence
{
    /// <summary>
    /// Autosave en eventos seguros (prompt §30): entrar a una zona nueva o terminar un
    /// diálogo importante, con un intervalo mínimo entre autosaves para evitar escrituras
    /// frecuentes/corrupción.
    /// </summary>
    public class AutosaveController : MonoBehaviour
    {
        [SerializeField] private float minSecondsBetweenAutosaves = 30f;
        [SerializeField] private string slotName = "autosave";

        private float lastAutosaveTime = -9999f;

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out LocationSystem locationSystem))
                locationSystem.OnLocationChanged += OnLocationChanged;

            if (ServiceLocator.TryGet(out DialogueRunner dialogueRunner))
                dialogueRunner.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet(out LocationSystem locationSystem))
                locationSystem.OnLocationChanged -= OnLocationChanged;

            if (ServiceLocator.TryGet(out DialogueRunner dialogueRunner))
                dialogueRunner.OnDialogueEnded -= OnDialogueEnded;
        }

        private void OnLocationChanged(LocationDefinition _) => TryAutosave();
        private void OnDialogueEnded() => TryAutosave();

        private void TryAutosave()
        {
            if (Time.unscaledTime - lastAutosaveTime < minSecondsBetweenAutosaves) return;
            lastAutosaveTime = Time.unscaledTime;
            ServiceLocator.Get<SaveSystem>().SaveToSlot(slotName);
        }
    }
}

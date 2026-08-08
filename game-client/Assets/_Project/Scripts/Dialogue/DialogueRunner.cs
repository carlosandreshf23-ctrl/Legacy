using System;
using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.Narrative;

namespace LegadoPeru.Dialogue
{
    /// <summary>
    /// MVP de sistema de diálogo (prompt §19): una única conversación activa a la vez,
    /// avance lineal o por elecciones, con enganche directo a DecisionService cuando un
    /// nodo tiene decisionId. Reutiliza el motor de decisiones en vez de crear uno nuevo
    /// para diálogo (coherente con 07_CONSEQUENCE_SYSTEM.md §4).
    /// </summary>
    public class DialogueRunner
    {
        public bool IsActive { get; private set; }
        public DialogueDefinition Current { get; private set; }
        public DialogueNode CurrentNode { get; private set; }

        public event Action<DialogueNode> OnNodeShown;
        public event Action OnDialogueEnded;
        public event Action<string, string> OnChoiceRecorded;

        private string activeCharacterId;
        private string activeLocationId;

        public void StartDialogue(DialogueDefinition definition, string characterId, string locationId)
        {
            if (definition == null)
            {
                DebugLog.LogWarning(DebugLog.Category.Narrative, "StartDialogue llamado con definition == null.");
                return;
            }

            Current = definition;
            activeCharacterId = characterId;
            activeLocationId = locationId;
            IsActive = true;

            ShowNode(definition.startNodeId);
        }

        public void Continue()
        {
            if (CurrentNode == null) return;
            if (CurrentNode.choices != null && CurrentNode.choices.Count > 0) return;
            ShowNode(CurrentNode.nextNodeId);
        }

        public void SelectChoice(int index)
        {
            if (CurrentNode?.choices == null || index < 0 || index >= CurrentNode.choices.Count) return;

            var choice = CurrentNode.choices[index];

            if (!string.IsNullOrEmpty(CurrentNode.decisionId) && !string.IsNullOrEmpty(choice.decisionChoiceValue))
            {
                ServiceLocator.Get<DecisionService>()
                    .RecordDecision(CurrentNode.decisionId, activeCharacterId, activeLocationId, choice.decisionChoiceValue);
                OnChoiceRecorded?.Invoke(CurrentNode.decisionId, choice.decisionChoiceValue);
            }

            ShowNode(choice.nextNodeId);
        }

        private void ShowNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                EndDialogue();
                return;
            }

            var node = Current.GetNode(nodeId);
            if (node == null)
            {
                DebugLog.LogWarning(DebugLog.Category.Narrative, $"Nodo de diálogo '{nodeId}' no encontrado; terminando diálogo.");
                EndDialogue();
                return;
            }

            CurrentNode = node;
            OnNodeShown?.Invoke(node);
        }

        private void EndDialogue()
        {
            IsActive = false;
            CurrentNode = null;
            Current = null;
            OnDialogueEnded?.Invoke();
        }
    }
}

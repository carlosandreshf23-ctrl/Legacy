using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegadoPeru.Dialogue
{
    /// <summary>Una opción de diálogo (prompt §19-20). decisionChoiceValue no vacío = esta elección alimenta un DecisionRecord.</summary>
    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextNodeId;
        public string decisionChoiceValue;
    }

    /// <summary>Un nodo de diálogo: texto lineal (nextNodeId) o con elecciones (choices).</summary>
    [Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public string speakerName;
        [TextArea] public string text;

        [Tooltip("No vacío si las elecciones de este nodo deben registrarse como decisión (DecisionService).")]
        public string decisionId;

        [Tooltip("Usado solo cuando choices está vacío (continuación lineal).")]
        public string nextNodeId;

        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    /// <summary>
    /// Diálogo data-driven (prompt §20): los textos nunca están hardcodeados en scripts.
    /// Preparado para condiciones/misiones/variables futuras vía decisionId + DecisionService.
    /// </summary>
    [CreateAssetMenu(menuName = "Legado/Dialogue Definition", fileName = "DialogueDefinition")]
    public class DialogueDefinition : ScriptableObject
    {
        public string dialogueId;
        public string startNodeId;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        public DialogueNode GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            return nodes.Find(n => n.nodeId == nodeId);
        }
    }
}

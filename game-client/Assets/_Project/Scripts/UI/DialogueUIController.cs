using LegadoPeru.Core;
using LegadoPeru.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>Panel de diálogo MVP (prompt §19): hablante, texto, continuar o hasta dos elecciones.</summary>
    public class DialogueUIController : MonoBehaviour
    {
        public GameObject panelRoot;
        public Text speakerText;
        public Text bodyText;
        public Button continueButton;
        public Button choiceButtonA;
        public Button choiceButtonB;
        public Text choiceTextA;
        public Text choiceTextB;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (continueButton != null)
                continueButton.onClick.AddListener(() => ServiceLocator.Get<DialogueRunner>().Continue());
            if (choiceButtonA != null)
                choiceButtonA.onClick.AddListener(() => ServiceLocator.Get<DialogueRunner>().SelectChoice(0));
            if (choiceButtonB != null)
                choiceButtonB.onClick.AddListener(() => ServiceLocator.Get<DialogueRunner>().SelectChoice(1));
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet(out DialogueRunner runner)) return;
            runner.OnNodeShown += ShowNode;
            runner.OnDialogueEnded += HideDialogue;
        }

        private void OnDisable()
        {
            if (!ServiceLocator.TryGet(out DialogueRunner runner)) return;
            runner.OnNodeShown -= ShowNode;
            runner.OnDialogueEnded -= HideDialogue;
        }

        private void ShowNode(DialogueNode node)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (speakerText != null) speakerText.text = node.speakerName;
            if (bodyText != null) bodyText.text = node.text;

            bool hasChoices = node.choices != null && node.choices.Count > 0;

            if (continueButton != null) continueButton.gameObject.SetActive(!hasChoices);
            if (choiceButtonA != null) choiceButtonA.gameObject.SetActive(hasChoices && node.choices.Count > 0);
            if (choiceButtonB != null) choiceButtonB.gameObject.SetActive(hasChoices && node.choices.Count > 1);

            if (hasChoices)
            {
                if (choiceTextA != null && node.choices.Count > 0) choiceTextA.text = node.choices[0].choiceText;
                if (choiceTextB != null && node.choices.Count > 1) choiceTextB.text = node.choices[1].choiceText;
            }
        }

        private void HideDialogue()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}

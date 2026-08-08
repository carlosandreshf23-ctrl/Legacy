using System.Collections.Generic;
using LegadoPeru.Core;
using LegadoPeru.Dialogue;
using LegadoPeru.Interaction;
using LegadoPeru.World;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// NPC básico (prompt §18): rutina extremadamente simple (patrulla 2-3 puntos, mira al
    /// jugador cuando está cerca) y diálogo mediante DialogueRunner. Su estado
    /// (HasTalked/relación de prueba) persiste vía PersistentWorldObject.
    /// </summary>
    public class NPCController : PersistentWorldObject, IInteractable
    {
        [SerializeField] private string npcId = "NPC_Test_01";
        [SerializeField] private DialogueDefinition dialogue;
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
        [SerializeField] private float patrolSpeed = 1.0f;
        [SerializeField] private float waitSecondsAtPoint = 3f;
        [SerializeField] private float lookAtPlayerRadius = 4f;

        private bool hasTalked;
        private bool relationshipFlag;
        private int currentPointIndex;
        private float waitTimer;
        private Transform playerTransform;

        public void SetNpcId(string id) => npcId = id;
        public void SetDialogue(DialogueDefinition def) => dialogue = def;
        public void SetPatrolPoints(List<Transform> points) => patrolPoints = points;

        public string InteractionPrompt => "Hablar";

        public bool CanInteract => !ServiceLocator.TryGet(out DialogueRunner runner) || !runner.IsActive;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out PlayerCharacterController player))
                playerTransform = player.transform;

            if (ServiceLocator.TryGet(out DialogueRunner runner))
                runner.OnChoiceRecorded += HandleChoiceRecorded;
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out DialogueRunner runner))
                runner.OnChoiceRecorded -= HandleChoiceRecorded;
        }

        private void Update()
        {
            if (playerTransform != null &&
                (playerTransform.position - transform.position).sqrMagnitude <= lookAtPlayerRadius * lookAtPlayerRadius)
            {
                LookAt(playerTransform.position);
                return;
            }

            Patrol();
        }

        private void Patrol()
        {
            if (patrolPoints.Count == 0) return;

            Transform destination = patrolPoints[currentPointIndex];
            if (destination == null) return;

            transform.position = Vector3.MoveTowards(transform.position, destination.position, patrolSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination.position) < 0.1f)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitSecondsAtPoint)
                {
                    waitTimer = 0f;
                    currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
                }
            }
        }

        private void LookAt(Vector3 worldPosition)
        {
            Vector3 dir = worldPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        }

        public void Interact(GameObject interactor)
        {
            var runner = ServiceLocator.Get<DialogueRunner>();
            var locationSystem = ServiceLocator.Get<LocationSystem>();
            runner.StartDialogue(dialogue, npcId, locationSystem.CurrentLocationId);
            hasTalked = true;
        }

        private void HandleChoiceRecorded(string decisionId, string value)
        {
            if (decisionId == "TestDecision_Honesty")
                relationshipFlag = value == "TRUE";
        }

        public override string CaptureState() => $"{hasTalked}|{relationshipFlag}";

        public override void RestoreState(string state)
        {
            if (string.IsNullOrEmpty(state)) return;

            var parts = state.Split('|');
            if (parts.Length != 2) return;

            bool.TryParse(parts[0], out hasTalked);
            bool.TryParse(parts[1], out relationshipFlag);
        }
    }
}

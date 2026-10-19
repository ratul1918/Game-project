using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberBlade.Missions
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private DialogueInteractable[] interactables;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private float interactionRange = 3f;

        private DialogueInteractable nearby;
        private DialogueData activeDialogue;
        private int lineIndex;
        private bool showing;

        public event Action<string> OnDialogueCompleted;

        public void Initialize(
            Transform playerTarget,
            DialogueInteractable[] targets,
            TMP_Text speaker,
            TMP_Text dialogue,
            TMP_Text prompt)
        {
            player = playerTarget;
            interactables = targets;
            speakerText = speaker;
            dialogueText = dialogue;
            promptText = prompt;
            Hide();
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            nearby = FindNearby();
            if (!showing)
            {
                promptText.text = nearby != null ? $"Press E to talk to {nearby.name}" : string.Empty;
                if (nearby != null && Keyboard.current != null
                    && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Begin(nearby);
                }
                return;
            }

            promptText.text = "Press E for next line";
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ShowNextLine();
            }
        }

        private DialogueInteractable FindNearby()
        {
            DialogueInteractable closest = null;
            float closestDistance = interactionRange;
            foreach (DialogueInteractable target in interactables ?? Array.Empty<DialogueInteractable>())
            {
                if (target == null || target.Dialogue == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(player.position, target.transform.position);
                if (distance <= closestDistance)
                {
                    closest = target;
                    closestDistance = distance;
                }
            }
            return closest;
        }

        private void Begin(DialogueInteractable target)
        {
            activeDialogue = target.Dialogue;
            lineIndex = 0;
            showing = true;
            ShowNextLine();
        }

        private void ShowNextLine()
        {
            if (activeDialogue == null)
            {
                return;
            }
            if (lineIndex >= activeDialogue.lines.Length)
            {
                Finish();
                return;
            }

            DialogueLine line = activeDialogue.lines[lineIndex++];
            speakerText.text = line.speaker;
            dialogueText.text = line.text;
            if (lineIndex >= activeDialogue.lines.Length)
            {
                promptText.text = "Press E to finish";
            }

        }

        private void Finish()
        {
            string id = activeDialogue.dialogueId;
            showing = false;
            activeDialogue = null;
            speakerText.text = string.Empty;
            dialogueText.text = string.Empty;
            OnDialogueCompleted?.Invoke(id);
        }

        private void Hide()
        {
            showing = false;
            if (speakerText != null) speakerText.text = string.Empty;
            if (dialogueText != null) dialogueText.text = string.Empty;
            if (promptText != null) promptText.text = string.Empty;
        }
    }
}

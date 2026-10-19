using UnityEngine;

namespace EmberBlade.Missions
{
    public class DialogueInteractable : MonoBehaviour
    {
        [SerializeField] private string dialogueId;
        [SerializeField] private DialogueData dialogue;

        public string DialogueId => dialogueId;
        public DialogueData Dialogue => dialogue;

        public void Initialize(string id, DialogueData data)
        {
            dialogueId = id;
            dialogue = data;
        }
    }
}

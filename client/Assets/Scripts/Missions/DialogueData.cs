using System;
using UnityEngine;

namespace EmberBlade.Missions
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)]
        public string text;
    }

    [CreateAssetMenu(fileName = "Dialogue", menuName = "Ember Blade/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public DialogueLine[] lines;
    }
}

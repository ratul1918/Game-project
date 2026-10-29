using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName = "The LightHouse/Logbook Page")]
    public sealed class LogbookPage : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea(8, 20)] public string body;
        public string storyFlag;
        public bool lockedUntilPhoto;
    }
}

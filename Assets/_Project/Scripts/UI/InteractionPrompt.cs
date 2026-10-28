using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class InteractionPrompt : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private string promptFormat = "[E] {0}";

        private void Awake()
        {
            Hide();
        }

        public void Show(string prompt)
        {
            if (label == null)
            {
                return;
            }

            label.text = string.Format(promptFormat, prompt);
            label.enabled = true;
        }

        public void Hide()
        {
            if (label != null)
            {
                label.enabled = false;
            }
        }
    }
}

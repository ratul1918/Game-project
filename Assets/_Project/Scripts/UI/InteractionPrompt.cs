using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheLightHouse
{
    public sealed class InteractionPrompt : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private TMP_Text styledLabel;
        [SerializeField] private Image keyIcon;
        [SerializeField] private string promptFormat = "[E] {0}";

        private void Awake()
        {
            Hide();
        }

        public void Show(string prompt)
        {
            if (label == null)
            {
                if(styledLabel==null)return;
            }

            string value=string.Format(promptFormat,prompt);
            if(label){label.text=value;label.enabled=styledLabel==null;}
            if(styledLabel){styledLabel.text=value;styledLabel.enabled=true;}
            if(keyIcon)keyIcon.enabled=true;
        }

        public void Hide()
        {
            if (label != null)
            {
                label.enabled = false;
            }
            if(styledLabel)styledLabel.enabled=false;
            if(keyIcon)keyIcon.enabled=false;
        }
    }
}

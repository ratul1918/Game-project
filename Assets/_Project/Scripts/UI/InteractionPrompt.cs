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
        CanvasGroup group;

        private void Awake()
        {
            group=GetComponent<CanvasGroup>()??gameObject.AddComponent<CanvasGroup>();
            Hide();
        }

        public void SetKeyIcon(Sprite sprite)
        {
            if(keyIcon){keyIcon.sprite=sprite;keyIcon.enabled=sprite&&group&&group.alpha>0f;}
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
            group.alpha=1f;
            group.blocksRaycasts=true;
        }

        public void Hide()
        {
            if (label != null)
            {
                label.enabled = false;
            }
            if(styledLabel)styledLabel.enabled=false;
            if(keyIcon)keyIcon.enabled=false;
            if(group){group.alpha=0f;group.blocksRaycasts=false;}
        }
    }
}

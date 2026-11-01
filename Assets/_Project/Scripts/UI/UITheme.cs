using TMPro;
using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/UI Theme",fileName="UITheme")]
    public sealed class UITheme : ScriptableObject
    {
        [Header("Colours")]
        [SerializeField] Color text=new(.91f,.92f,.88f,1f);
        [SerializeField] Color mutedText=new(.58f,.64f,.64f,1f);
        [SerializeField] Color panel=new(.025f,.035f,.04f,.86f);
        [SerializeField] Color accent=new(.84f,.61f,.29f,1f);
        [SerializeField] Color safe=new(.33f,.68f,.54f,1f);
        [SerializeField] Color warning=new(.9f,.62f,.22f,1f);
        [SerializeField] Color danger=new(.68f,.16f,.13f,1f);

        [Header("Typography")]
        [SerializeField] TMP_FontAsset bodyFont;
        [SerializeField] TMP_FontAsset displayFont;

        [Header("Sprites")]
        [SerializeField] Sprite panelSprite;
        [SerializeField] Sprite keySprite;
        [SerializeField] Sprite pageSprite;
        [SerializeField] Sprite itemSprite;
        [SerializeField] Sprite lampSprite;
        [SerializeField] Sprite checkpointSprite;

        public Color Text=>text;
        public Color MutedText=>mutedText;
        public Color Panel=>panel;
        public Color Accent=>accent;
        public Color Safe=>safe;
        public Color Warning=>warning;
        public Color Danger=>danger;
        public TMP_FontAsset BodyFont=>bodyFont;
        public TMP_FontAsset DisplayFont=>displayFont?displayFont:bodyFont;
        public Sprite PanelSprite=>panelSprite;
        public Sprite KeySprite=>keySprite;
        public Sprite PageSprite=>pageSprite;
        public Sprite ItemSprite=>itemSprite;
        public Sprite LampSprite=>lampSprite;
        public Sprite CheckpointSprite=>checkpointSprite;
    }
}

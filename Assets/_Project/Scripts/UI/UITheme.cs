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
        [SerializeField] Color coldBlueGrey=new(.42f,.54f,.62f,1f);
        [SerializeField] Color amber=new(.88f,.58f,.22f,1f);

        [Header("Typography")]
        [SerializeField] TMP_FontAsset bodyFont;
        [SerializeField] TMP_FontAsset displayFont;
        [SerializeField] TMP_FontAsset journalFont;
        [SerializeField] TMP_FontAsset notesFont;

        [Header("Sprites")]
        [SerializeField] Sprite panelSprite;
        [SerializeField] Sprite keySprite;
        [SerializeField] Sprite pageSprite;
        [SerializeField] Sprite itemSprite;
        [SerializeField] Sprite lampSprite;
        [SerializeField] Sprite checkpointSprite;
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Sprite barFrameSprite;
        [SerializeField] Sprite barFillSprite;
        [SerializeField] Sprite toastSprite;
        [SerializeField] Sprite tabSprite;
        [SerializeField] Sprite fuelSprite;
        [SerializeField] Sprite ledgerSprite;
        [SerializeField] Sprite lanternSprite;
        [SerializeField] Sprite hideSprite;
        [SerializeField] Sprite crankSprite;
        [SerializeField] Sprite interactSprite;
        [SerializeField] Vector4 typeScale=new(14f,18f,24f,36f);

        public Color Text=>text;
        public Color MutedText=>mutedText;
        public Color Panel=>panel;
        public Color Accent=>accent;
        public Color Safe=>safe;
        public Color Warning=>warning;
        public Color Danger=>danger;
        public Color ColdBlueGrey=>coldBlueGrey;
        public Color Amber=>amber;
        public TMP_FontAsset BodyFont=>bodyFont;
        public TMP_FontAsset DisplayFont=>displayFont?displayFont:bodyFont;
        public TMP_FontAsset JournalFont=>journalFont?journalFont:bodyFont;
        public TMP_FontAsset NotesFont=>notesFont?notesFont:bodyFont;
        public Sprite PanelSprite=>panelSprite;
        public Sprite KeySprite=>keySprite;
        public Sprite PageSprite=>pageSprite;
        public Sprite ItemSprite=>itemSprite;
        public Sprite LampSprite=>lampSprite;
        public Sprite CheckpointSprite=>checkpointSprite;
        public Sprite ButtonSprite=>buttonSprite;
        public Sprite BarFrameSprite=>barFrameSprite;
        public Sprite BarFillSprite=>barFillSprite;
        public Sprite ToastSprite=>toastSprite;
        public Sprite TabSprite=>tabSprite;
        public Sprite FuelSprite=>fuelSprite;
        public Sprite LedgerSprite=>ledgerSprite;
        public Sprite LanternSprite=>lanternSprite;
        public Sprite HideSprite=>hideSprite;
        public Sprite CrankSprite=>crankSprite;
        public Sprite InteractSprite=>interactSprite;
        public Vector4 TypeScale=>typeScale;
    }
}

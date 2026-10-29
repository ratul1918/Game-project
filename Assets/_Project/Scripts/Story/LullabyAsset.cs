using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/Lullaby")]
    public sealed class LullabyAsset : ScriptableObject
    {
        [TextArea] public string[] placeholderLines;
        [TextArea] public string finalLine="FINAL LINE — Sleep now, my light; the sea is still.";
        public AudioClip hum;
        public AudioClip finalLineClip;
    }
}

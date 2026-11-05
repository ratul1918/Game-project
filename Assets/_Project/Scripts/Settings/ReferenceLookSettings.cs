using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName = "The LightHouse/Reference Look")]
    public sealed class ReferenceLookSettings : ScriptableObject
    {
        public Color plaster = new(.32f, .40f, .38f);
        public Color wood = new(.23f, .19f, .13f);
        public Color iron = new(.15f, .21f, .22f);
        public Color moon = new(.56f, .72f, .77f);
        public Color practical = new(1f, .73f, .40f);
        [Range(0, 2)] public float moonIntensity = .42f;
        [Range(0, 5)] public float practicalIntensity = 2.2f;
        [Range(0, .1f)] public float fogDensity = .018f;
        [Range(0, 1)] public float surfaceWear = .72f;
        [Range(0, 1)] public float vignette = .24f;
        [Range(0, 1)] public float grain = .16f;
        [Range(0, 200)] public int scrubCount = 85;
    }
}

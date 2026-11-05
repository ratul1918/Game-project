using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/Island Arrival")]
    public sealed class IslandArrivalSettings : ScriptableObject
    {
        public Vector3 playerStart=new(0,.22f,-24);
        public Vector3 boatPosition=new(-4.6f,-.45f,-31.5f);
        public Vector3 departureOffset=new(-24,0,-20);
        public float boatmanHeight=1.65f;
        public Vector2 islandRadius=new(24,21);
        public float landHeight=.1f;
        public float seaLevel=-.55f;
        [Range(16,128)] public int shoreSegments=64;
        [Range(8,64)] public int shoreRockCount=26;
        [Min(100)] public float seaSize=500;
        [Range(16,128)] public int seaSegments=64;
        [Range(0,.4f)] public float waveHeight=.13f;
    }
}

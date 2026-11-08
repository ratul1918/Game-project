using UnityEngine;
namespace TheLightHouse
{
    [CreateAssetMenu(menuName = "The LightHouse/Level/Architecture Repair Settings")]
    public sealed class ArchitectureRepairSettings : ScriptableObject
    {
        [Min(1.3f)] public float stairWidth = 1.35f;
        public float stairOuterRadius = 4.60f;
        [Min(2.6f)] public float headClearance = 2.6f;
        [Min(1.1f)] public float roomDoorWidth = 1.1f;
        [Min(2.2f)] public float roomDoorHeight = 2.2f;
        [Min(1.2f)] public float cellarWidth = 1.4f;
        public float cellarDescent = 4.5f;
        public Vector3 torchAnchor = new(0.24f, -0.22f, 0.45f);
    }
}

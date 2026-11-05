using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/Level/Lighthouse Layout Settings",fileName="LighthouseLayoutSettings")]
    public sealed class LighthouseLayoutSettings : ScriptableObject
    {
        [Header("Tower")]
        [Min(8f)] public float towerHeight=28f;
        [Min(4f)] public float towerBaseDiameter=8f;
        [Min(3f)] public float towerGalleryDiameter=5f;
        [Min(.2f)] public float towerBaseWallThickness=.8f;
        [Min(.1f)] public float towerGalleryWallThickness=.45f;
        [Min(8)] public int towerWallSegments=24;
        [Min(2f)] public float floorHeight=3.2f;
        [Min(1)] public int towerLevelCount=8;
        [Min(.1f)] public float ringCorniceHeight=.22f;
        [Min(.1f)] public float ringCorniceDepth=.22f;
        [Min(8)] public int lanternPaneCount=16;
        [Min(.1f)] public float lanternPaneWidth=.22f;
        [Min(.1f)] public float galleryWidth=1.5f;
        [Min(.1f)] public float railingHeight=1.1f;
        [Min(.1f)] public float stairStepHeight=.18f;
        [Min(.1f)] public float stairStepDepth=.28f;
        [Min(1f)] public float stairWidth=1.1f;
        [Min(.5f)] public float stairwellRadius=1.05f;
        [Range(1f,1.02f)] public float wallSegmentCoverage=1.001f;

        [Header("Keeper House")]
        public Vector3 houseSize=new(14f,2.8f,8f);
        [Min(.1f)] public float houseWallThickness=.3f;
        [Min(.1f)] public float houseDoorWidth=1.8f;
        [Min(.1f)] public float houseDoorHeight=2.4f;
        [Min(.1f)] public float towerDoorWidth=1.4f;
        [Min(.1f)] public float towerDoorHeight=2.2f;
        [Min(.1f)] public float roofPitch=2.2f;

        [Header("Basement and setting")]
        public float basementDepth=3.2f;
        [Min(1)] public int cliffRockCount=9;
        [Min(4)] public int outsideStairSteps=32;
        [Min(2)] public int occlusionFloors=10;
    }
}

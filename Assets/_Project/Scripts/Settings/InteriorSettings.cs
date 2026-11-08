using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(fileName = "InteriorSettings", menuName = "TheLightHouse/Interior Settings")]
    public sealed class InteriorSettings : ScriptableObject
    {
        [Header("Grid and Dimensions")]
        [Min(0.1f)] public float gridUnit = 0.5f;
        [Min(1f)] public float floorHeight = 3.2f;
        [Min(1f)] public float towerBaseDiameter = 9.0f;
        [Min(1f)] public float towerShaftSize = 5.0f;
        public Vector3 keeperHouseDimensions = new Vector3(14.0f, 6.4f, 8.0f);

        [Header("Stairs")]
        public int stepsPerFlight = 9;
        public float stepRise = 0.178f;
        public float stepRun = 0.28f;
        public float stairWidth = 1.2f;
        public Vector2 landingDimensions = new Vector2(1.24f, 2.5f);
        public float minHeadroom = 2.4f;
        public float rampColliderAngle = 32.44f;

        [Header("Tower Levels")]
        public int stairOnlyLevels = 3;

        [Header("Cellar & Vault")]
        public float cellarDepth = 5.0f;
        public float vaultWidth = 2.8f;
        public float vaultHeight = 2.6f;
        public float vaultCorridorLength = 20.0f;
        public Vector2 hiddenRoomDimensions = new Vector2(4.0f, 4.0f);

        [Header("Materials")]
        public Material stoneWallMaterial;
        public Material flagstoneMaterial;
        public Material plasterMaterial;
        public Material woodMaterial;
        public Material rustMetalMaterial;
        public Material wetRockMaterial;
        public Material sandMaterial;
        public Material paperMaterial;

        [Header("Prefabs")]
        public GameObject wallPrefab;
        public GameObject floorSlabPrefab;
        public GameObject doorFramePrefab;
        public GameObject stairFlightPrefab;
        public GameObject landingPrefab;
        public GameObject handrailPrefab;
        public GameObject windowFramePrefab;
        public GameObject vaultSegmentPrefab;
    }
}

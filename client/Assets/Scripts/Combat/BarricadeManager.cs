using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EmberBlade.Config;
using EmberBlade.AI;

namespace EmberBlade.Combat
{
    public class BarricadeManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Camera placementCamera;
        [SerializeField] private Transform placementRoot;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text inventoryText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material trapMaterial;
        [SerializeField] private EnemyWaveManager enemyWaveManager;

        private readonly List<GameObject> placedPieces = new();
        private GameObject ghost;
        private BarricadePieceType selectedType = BarricadePieceType.Wall;
        private float remainingBuildTime;
        private float rotation;
        private bool buildPhase = true;
        private int wallsPlaced;
        private int trapsPlaced;

        public bool BuildPhaseActive => buildPhase;

        public void ResetBuildPhase()
        {
            foreach (GameObject piece in placedPieces)
            {
                if (piece != null)
                {
                    Destroy(piece);
                }
            }
            placedPieces.Clear();
            wallsPlaced = 0;
            trapsPlaced = 0;
            remainingBuildTime = config != null ? config.barricadeBuildTime : 0f;
            buildPhase = true;
            if (ghost != null)
            {
                ghost.SetActive(true);
            }
            UpdateHud();
        }

        public void Initialize(
            GameConfig gameConfig,
            Camera camera,
            Transform root,
            TMP_Text timer,
            TMP_Text inventory,
            TMP_Text instructions,
            Material wall,
            Material trap,
            EnemyWaveManager enemies)
        {
            config = gameConfig;
            placementCamera = camera;
            placementRoot = root;
            timerText = timer;
            inventoryText = inventory;
            instructionText = instructions;
            wallMaterial = wall;
            trapMaterial = trap;
            enemyWaveManager = enemies;
            if (config == null || placementCamera == null || placementRoot == null
                || timerText == null || inventoryText == null || instructionText == null
                || wallMaterial == null || trapMaterial == null || enemyWaveManager == null)
            {
                Debug.LogError($"BarricadeManager on '{name}' is missing a required serialized reference.", this);
                enabled = false;
                return;
            }
            remainingBuildTime = config.barricadeBuildTime;
            EnsureGhost();
            UpdateHud();
        }

        private void Update()
        {
            if (!buildPhase)
            {
                return;
            }

            remainingBuildTime = Mathf.Max(0f, remainingBuildTime - Time.deltaTime);
            UpdateHud();
            if (remainingBuildTime <= 0f)
            {
                StartRaid();
                return;
            }

            UpdateGhost();
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                rotation = Mathf.Repeat(rotation + 90f, 360f);
            }
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                selectedType = selectedType == BarricadePieceType.Wall
                    ? BarricadePieceType.SpikeTrap
                    : BarricadePieceType.Wall;
            }
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                StartRaid();
                return;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceSelected();
            }
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                RemoveSelected();
            }
        }

        public void InitializeFromBuilder(GameConfig gameConfig, Camera camera, Transform root,
            TMP_Text timer, TMP_Text inventory, TMP_Text instructions, Material wall, Material trap,
            EnemyWaveManager enemies)
        {
            Initialize(gameConfig, camera, root, timer, inventory, instructions, wall, trap, enemies);
        }

        private void EnsureGhost()
        {
            ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ghost.name = "BarricadeGhost";
            ghost.transform.SetParent(transform, false);
            ghost.GetComponent<Collider>().enabled = false;
            ghost.GetComponent<Renderer>().material = wallMaterial;
            ghost.GetComponent<Renderer>().material.color = new Color(0.2f, 1f, 0.2f, 0.5f);
        }

        private void UpdateGhost()
        {
            if (placementCamera == null || ghost == null)
            {
                return;
            }

            Ray ray = placementCamera.ScreenPointToRay(Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : new Vector2(Screen.width / 2f, Screen.height / 2f));
            if (!Physics.Raycast(ray, out RaycastHit hit, config.barricadePlacementRange * 2f))
            {
                ghost.SetActive(false);
                return;
            }

            Vector3 point = Snap(hit.point);
            bool valid = IsValidPlacement(point);
            ghost.SetActive(true);
            ghost.transform.position = point + Vector3.up * (config.barricadeWallHeight * 0.5f);
            ghost.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            ghost.transform.localScale = selectedType == BarricadePieceType.Wall
                ? new Vector3(config.barricadeWallWidth, config.barricadeWallHeight, 0.35f)
                : new Vector3(1.2f, 0.15f, 1.2f);
            ghost.GetComponent<Renderer>().material.color = valid
                ? new Color(0.2f, 1f, 0.2f, 0.5f)
                : new Color(1f, 0.2f, 0.2f, 0.5f);
        }

        private void PlaceSelected()
        {
            if (ghost == null || !ghost.activeSelf || !IsValidPlacement(ghost.transform.position))
            {
                return;
            }

            bool placingWall = selectedType == BarricadePieceType.Wall;
            if (!CanPlaceWithinLimit(selectedType, wallsPlaced, trapsPlaced, config))
            {
                return;
            }

            GameObject piece = GameObject.CreatePrimitive(placingWall ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            piece.name = placingWall ? "BarricadeWall" : "SpikeTrap";
            piece.transform.SetParent(placementRoot, false);
            piece.transform.position = ghost.transform.position;
            piece.transform.rotation = ghost.transform.rotation;
            piece.transform.localScale = ghost.transform.localScale;
            piece.GetComponent<Renderer>().material = placingWall ? wallMaterial : trapMaterial;
            BarricadePiece barricade = piece.AddComponent<BarricadePiece>();
            barricade.Initialize(selectedType, config);
            if (!placingWall)
            {
                SphereCollider trigger = piece.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;
            }

            placedPieces.Add(piece);
            if (placingWall)
            {
                wallsPlaced++;
            }
            else
            {
                trapsPlaced++;
            }
            UpdateHud();
        }

        private void RemoveSelected()
        {
            if (placementCamera == null)
            {
                return;
            }

            Ray ray = placementCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, config.barricadePlacementRange * 2f))
            {
                return;
            }

            BarricadePiece piece = hit.collider.GetComponentInParent<BarricadePiece>();
            if (piece == null)
            {
                return;
            }

            placedPieces.Remove(piece.gameObject);
            if (piece.PieceType == BarricadePieceType.Wall)
            {
                wallsPlaced--;
            }
            else
            {
                trapsPlaced--;
            }
            Destroy(piece.gameObject);
            UpdateHud();
        }

        private void StartRaid()
        {
            if (!buildPhase)
            {
                return;
            }
            buildPhase = false;
            ghost.SetActive(false);
            BarricadeResult.Record(wallsPlaced, trapsPlaced);
            enemyWaveManager.BeginWave();
            instructionText.text = "Raid started. Defend Emberfield!";
        }

        private bool IsValidPlacement(Vector3 point)
        {
            if (Vector3.Distance(transform.position, point) > config.barricadePlacementRange)
            {
                return false;
            }

            foreach (GameObject piece in placedPieces)
            {
                if (piece != null && Vector3.Distance(piece.transform.position, point) < config.barricadeGridSize * 0.75f)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CanPlaceWithinLimit(
            BarricadePieceType type,
            int currentWalls,
            int currentTraps,
            GameConfig gameConfig)
        {
            if (gameConfig == null)
            {
                return false;
            }

            return type == BarricadePieceType.Wall
                ? currentWalls < gameConfig.maxBarricadeWalls
                : currentTraps < gameConfig.maxBarricadeTraps;
        }

        private Vector3 Snap(Vector3 point)
        {
            float grid = config.barricadeGridSize;
            return new Vector3(Mathf.Round(point.x / grid) * grid, 0f, Mathf.Round(point.z / grid) * grid);
        }

        private void UpdateHud()
        {
            if (timerText != null)
            {
                timerText.text = $"Build time: {Mathf.CeilToInt(remainingBuildTime)}";
            }
            if (inventoryText != null)
            {
                inventoryText.text = $"Walls: {config.maxBarricadeWalls - wallsPlaced}  Traps: {config.maxBarricadeTraps - trapsPlaced}";
            }
            if (instructionText != null)
            {
                instructionText.text = "Click: place wall | T: select trap | R: rotate | Right-click: remove | Enter: raid";
            }
        }
    }
}

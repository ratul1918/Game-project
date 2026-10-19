using TMPro;
using UnityEngine;
using EmberBlade.AI;
using EmberBlade.Combat;
using EmberBlade.Config;
using EmberBlade.Player;

namespace EmberBlade.Missions
{
    public class Chapter1SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameObject player;
        [SerializeField] private ThirdPersonCamera followCamera;
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private BarricadeManager barricadeManager;
        [SerializeField] private EnemyWaveManager enemyWaveManager;
        [SerializeField] private CombatHudController combatHud;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject skirmisherPrefab;
        [SerializeField] private GameObject smokeObject;
        [SerializeField] private GameObject royalRider;
        [SerializeField] private DialogueInteractable[] dialogueTargets;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text inventoryText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Camera placementCamera;
        [SerializeField] private Transform placementRoot;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material trapMaterial;

        private void Start()
        {
            config ??= GameConfig.LoadRuntime();
            Health health = player.GetComponent<Health>();
            EmberMeter ember = player.GetComponent<EmberMeter>();
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            health.Initialize(config.playerMaxHealth);
            ember.Initialize(config);
            combat.Initialize(config, health, ember);
            followCamera.Initialize(config, player.transform);
            enemyWaveManager.Initialize(
                config, player.transform, combat, scoutPrefab, skirmisherPrefab, null, false);
            barricadeManager.Initialize(
                config, placementCamera, placementRoot, timerText, inventoryText,
                instructionText, wallMaterial, trapMaterial, enemyWaveManager);
            barricadeManager.enabled = false;
            combatHud.Initialize(health, ember, enemyWaveManager);
            dialogueUI.Initialize(player.transform, dialogueTargets, speakerText, dialogueText, promptText);
            missionManager.Initialize(
                dialogueUI, barricadeManager, enemyWaveManager, health, smokeObject,
                royalRider, objectiveText, resultText);
        }
    }
}

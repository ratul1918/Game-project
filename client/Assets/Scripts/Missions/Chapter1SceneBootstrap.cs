using TMPro;
using UnityEngine;
using EmberBlade.AI;
using EmberBlade.Combat;
using EmberBlade.Config;
using EmberBlade.Player;
using EmberBlade.UI;

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
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }
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
            dialogueUI.Initialize(
                player.transform, dialogueTargets, speakerText, dialogueText, promptText, config);
            missionManager.Initialize(
                dialogueUI, barricadeManager, enemyWaveManager, health, smokeObject,
                royalRider, objectiveText, resultText, config);
        }

        private bool ValidateReferences()
        {
            bool valid = true;
            valid &= Require(config, "config");
            valid &= Require(player, "player");
            valid &= Require(followCamera, "followCamera");
            valid &= Require(dialogueUI, "dialogueUI");
            valid &= Require(missionManager, "missionManager");
            valid &= Require(barricadeManager, "barricadeManager");
            valid &= Require(enemyWaveManager, "enemyWaveManager");
            valid &= Require(combatHud, "combatHud");
            valid &= Require(scoutPrefab, "scoutPrefab");
            valid &= Require(skirmisherPrefab, "skirmisherPrefab");
            valid &= Require(smokeObject, "smokeObject");
            valid &= Require(royalRider, "royalRider");
            valid &= Require(speakerText, "speakerText");
            valid &= Require(dialogueText, "dialogueText");
            valid &= Require(promptText, "promptText");
            valid &= Require(objectiveText, "objectiveText");
            valid &= Require(resultText, "resultText");
            valid &= Require(timerText, "timerText");
            valid &= Require(inventoryText, "inventoryText");
            valid &= Require(instructionText, "instructionText");
            valid &= Require(placementCamera, "placementCamera");
            valid &= Require(placementRoot, "placementRoot");
            valid &= Require(wallMaterial, "wallMaterial");
            valid &= Require(trapMaterial, "trapMaterial");
            if (player != null)
            {
                valid &= Require(player.GetComponent<Health>(), "player.Health");
                valid &= Require(player.GetComponent<EmberMeter>(), "player.EmberMeter");
                valid &= Require(player.GetComponent<PlayerCombat>(), "player.PlayerCombat");
            }
            return valid;
        }

        private bool Require(Object value, string fieldName)
        {
            if (value != null)
            {
                return true;
            }
            Debug.LogError($"Chapter1SceneBootstrap is missing '{fieldName}' on '{name}'. " +
                           "Run Ember Blade > Build Chapter 1 Scene.", this);
            return false;
        }
    }
}

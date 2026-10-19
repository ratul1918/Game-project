using UnityEngine;
using EmberBlade.AI;
using EmberBlade.Config;
using EmberBlade.Player;
using EmberBlade.UI;

namespace EmberBlade.Combat
{
    public class CombatSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameObject player;
        [SerializeField] private ThirdPersonCamera followCamera;
        [SerializeField] private EnemyWaveManager enemyWaveManager;
        [SerializeField] private CombatHudController combatHud;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject skirmisherPrefab;

        private void Start()
        {
            config ??= GameConfig.LoadRuntime();
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            Health playerHealth = player.GetComponent<Health>();
            EmberMeter emberMeter = player.GetComponent<EmberMeter>();
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            PlayerController controller = player.GetComponent<PlayerController>();

            playerHealth.Initialize(config.playerMaxHealth);
            emberMeter.Initialize(config);
            combat.Initialize(config, playerHealth, emberMeter);
            followCamera.Initialize(config, player.transform);
            enemyWaveManager.Initialize(
                config,
                player.transform,
                combat,
                scoutPrefab,
                skirmisherPrefab,
                null);
            combatHud.Initialize(playerHealth, emberMeter, enemyWaveManager);
            controller.enabled = true;
        }

        private bool ValidateReferences()
        {
            bool valid = true;
            valid &= Require(config, "config");
            valid &= Require(player, "player");
            valid &= Require(followCamera, "followCamera");
            valid &= Require(enemyWaveManager, "enemyWaveManager");
            valid &= Require(combatHud, "combatHud");
            valid &= Require(scoutPrefab, "scoutPrefab");
            valid &= Require(skirmisherPrefab, "skirmisherPrefab");
            if (player != null)
            {
                valid &= Require(player.GetComponent<Health>(), "player.Health component");
                valid &= Require(player.GetComponent<EmberMeter>(), "player.EmberMeter component");
                valid &= Require(player.GetComponent<PlayerCombat>(), "player.PlayerCombat component");
                valid &= Require(player.GetComponent<PlayerController>(), "player.PlayerController component");
            }
            return valid;
        }

        private bool Require(Object value, string fieldName)
        {
            if (value != null)
            {
                return true;
            }

            Debug.LogError($"CombatSceneBootstrap is missing '{fieldName}' on '{name}'. " +
                           "Run Ember Blade > Build Combat Test Scene.", this);
            return false;
        }
    }
}

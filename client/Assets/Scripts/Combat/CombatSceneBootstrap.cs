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
    }
}

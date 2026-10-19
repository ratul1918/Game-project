using UnityEngine;
using EmberBlade.AI;
using EmberBlade.Config;
using EmberBlade.Player;
using EmberBlade.UI;

namespace EmberBlade.Combat
{
    public class BarricadeSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameObject player;
        [SerializeField] private ThirdPersonCamera followCamera;
        [SerializeField] private EnemyWaveManager enemyWaveManager;
        [SerializeField] private BarricadeManager barricadeManager;
        [SerializeField] private CombatHudController combatHud;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject skirmisherPrefab;
        [SerializeField] private Camera placementCamera;
        [SerializeField] private Transform placementRoot;
        [SerializeField] private TMPro.TMP_Text timerText;
        [SerializeField] private TMPro.TMP_Text inventoryText;
        [SerializeField] private TMPro.TMP_Text instructionText;
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
                config,
                placementCamera,
                placementRoot,
                timerText,
                inventoryText,
                instructionText,
                wallMaterial,
                trapMaterial,
                enemyWaveManager);
            combatHud.Initialize(health, ember, enemyWaveManager);
        }
    }
}

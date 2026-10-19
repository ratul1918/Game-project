using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using EmberBlade.Config;
using EmberBlade.Combat;
using EmberBlade.Player;

namespace EmberBlade.AI
{
    public class EnemyWaveManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject skirmisherPrefab;
        [SerializeField] private Transform spawnRoot;

        private readonly HashSet<EnemyController> aliveEnemies = new();
        private Action<int> countChanged;

        public int EnemiesRemaining => aliveEnemies.Count;

        public void Initialize(
            GameConfig gameConfig,
            Transform playerTarget,
            PlayerCombat combat,
            GameObject scout,
            GameObject skirmisher,
            Action<int> onCountChanged)
        {
            config = gameConfig;
            player = playerTarget;
            playerCombat = combat;
            scoutPrefab = scout;
            skirmisherPrefab = skirmisher;
            countChanged = onCountChanged;
            SpawnWave();
        }

        private void SpawnWave()
        {
            for (int index = 0; index < config.scoutCount; index++)
            {
                SpawnEnemy(scoutPrefab, new Vector3(-5f + index * 2f, 0f, 8f));
            }
            for (int index = 0; index < config.skirmisherCount; index++)
            {
                SpawnEnemy(skirmisherPrefab, new Vector3(-2f + index * 4f, 0f, 12f));
            }
            countChanged?.Invoke(aliveEnemies.Count);
        }

        private void SpawnEnemy(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, spawnRoot);
            EnemyController enemy = instance.GetComponent<EnemyController>();
            bool skirmisher = instance.name.Contains("Skirmisher");
            enemy.Initialize(config, skirmisher, player, playerCombat, this);
            aliveEnemies.Add(enemy);
        }

        public void EnemyDied(EnemyController enemy)
        {
            aliveEnemies.Remove(enemy);
            countChanged?.Invoke(aliveEnemies.Count);
        }
    }
}

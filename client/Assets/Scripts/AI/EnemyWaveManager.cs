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
            Action<int> onCountChanged,
            bool spawnImmediately = true)
        {
            config = gameConfig;
            player = playerTarget;
            playerCombat = combat;
            scoutPrefab = scout;
            skirmisherPrefab = skirmisher;
            countChanged = onCountChanged;
            if (!ValidateReferences())
            {
                return;
            }
            if (spawnImmediately)
            {
                BeginWave();
            }
        }

        public void BeginWave()
        {
            if (aliveEnemies.Count > 0)
            {
                return;
            }

            SpawnWave();
        }

        public void ResetWave()
        {
            foreach (EnemyController enemy in new List<EnemyController>(aliveEnemies))
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            aliveEnemies.Clear();
            countChanged?.Invoke(0);
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
            if (!NavMesh.SamplePosition(position, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            {
                Debug.LogError($"Could not find a NavMesh spawn point near {position} for '{prefab.name}'. " +
                               "Rebuild CombatTest so the ground NavMesh is baked.", this);
                return;
            }

            Vector3 spawnPosition = navHit.position + Vector3.up;
            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, spawnRoot);
            EnemyController enemy = instance.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogError($"Enemy prefab '{prefab.name}' is missing EnemyController.", prefab);
                Destroy(instance);
                return;
            }
            bool skirmisher = instance.name.Contains("Skirmisher");
            enemy.Initialize(config, skirmisher, player, playerCombat, this);
            aliveEnemies.Add(enemy);
            Debug.Log($"Spawned {instance.name} at {spawnPosition}. Enemies remaining: {aliveEnemies.Count}", instance);
        }

        private bool ValidateReferences()
        {
            bool valid = true;
            valid &= Require(config, "config");
            valid &= Require(player, "player");
            valid &= Require(playerCombat, "playerCombat");
            valid &= Require(scoutPrefab, "scoutPrefab");
            valid &= Require(skirmisherPrefab, "skirmisherPrefab");
            if (spawnRoot == null)
            {
                spawnRoot = transform;
            }
            return valid;
        }

        private bool Require(UnityEngine.Object value, string fieldName)
        {
            if (value != null)
            {
                return true;
            }

            Debug.LogError($"EnemyWaveManager is missing '{fieldName}' on '{name}'. " +
                           "Run Ember Blade > Build Combat Test Scene.", this);
            return false;
        }

        public void EnemyDied(EnemyController enemy)
        {
            aliveEnemies.Remove(enemy);
            countChanged?.Invoke(aliveEnemies.Count);
        }
    }
}

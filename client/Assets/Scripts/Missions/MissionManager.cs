using System;
using TMPro;
using UnityEngine;
using EmberBlade.AI;
using EmberBlade.Combat;

namespace EmberBlade.Missions
{
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private BarricadeManager barricadeManager;
        [SerializeField] private EnemyWaveManager enemyWaveManager;
        [SerializeField] private Health playerHealth;
        [SerializeField] private GameObject smokeObject;
        [SerializeField] private GameObject royalRider;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text resultText;

        private readonly string[] harvestDialogues =
        {
            "tomas", "mara", "villager-one", "villager-two", "lira"
        };
        private int completedHarvestDialogues;
        private bool raidStarted;
        private bool mission2Restarting;
        private int raidWave;
        private string currentMission;

        public event Action<string> OnMissionStarted;
        public event Action<string, bool> OnMissionCompleted;

        public string CurrentMission => currentMission;

        public void Initialize(
            DialogueUI dialogue,
            BarricadeManager barricade,
            EnemyWaveManager enemies,
            Health health,
            GameObject smoke,
            GameObject rider,
            TMP_Text objective,
            TMP_Text result)
        {
            dialogueUI = dialogue;
            barricadeManager = barricade;
            enemyWaveManager = enemies;
            playerHealth = health;
            smokeObject = smoke;
            royalRider = rider;
            objectiveText = objective;
            resultText = result;
            dialogueUI.OnDialogueCompleted += HandleDialogueCompleted;
            playerHealth.OnDied += HandlePlayerDied;
            barricadeManager.enabled = false;
            royalRider.SetActive(false);
            smokeObject.SetActive(false);
            StartMission("1.1");
        }

        private void Update()
        {
            if (currentMission == "1.2" && !raidStarted
                && barricadeManager != null && !barricadeManager.BuildPhaseActive)
            {
                raidStarted = true;
                raidWave = 1;
                objectiveText.text = "Defeat all Silverhold raiders.";
            }

            if (currentMission == "1.2" && raidStarted
                && enemyWaveManager != null && enemyWaveManager.EnemiesRemaining == 0)
            {
                if (raidWave == 1)
                {
                    raidWave = 2;
                    objectiveText.text = "Defeat the second wave of Silverhold raiders.";
                    enemyWaveManager.BeginWave();
                }
                else if (raidWave == 2)
                {
                    CompleteMission("1.2", true);
                    StartMission("1.3");
                }
            }
        }

        private void StartMission(string missionId)
        {
            currentMission = missionId;
            OnMissionStarted?.Invoke(missionId);
            if (missionId == "1.1")
            {
                completedHarvestDialogues = 0;
                objectiveText.text = "Speak with Tomas, Mara, the two villagers, and Lira.";
                resultText.text = string.Empty;
            }
            else if (missionId == "1.2")
            {
                raidStarted = false;
                raidWave = 0;
                barricadeManager.enabled = true;
                objectiveText.text = "Build defenses before the raid.";
            }
            else
            {
                royalRider.SetActive(true);
                objectiveText.text = "Speak with the royal rider.";
            }
        }

        private void HandleDialogueCompleted(string dialogueId)
        {
            if (currentMission == "1.1")
            {
                for (int index = 0; index < harvestDialogues.Length; index++)
                {
                    if (harvestDialogues[index] == dialogueId)
                    {
                        completedHarvestDialogues++;
                        break;
                    }
                }

                if (completedHarvestDialogues >= harvestDialogues.Length)
                {
                    smokeObject.SetActive(true);
                    CompleteMission("1.1", true);
                    StartMission("1.2");
                }
            }
            else if (currentMission == "1.3" && dialogueId == "royal-rider")
            {
                CompleteMission("1.3", true);
                objectiveText.text = "Chapter 1 complete.";
                resultText.text = "Chapter 1 complete";
            }
        }

        private void HandlePlayerDied()
        {
            if (currentMission == "1.2" && !mission2Restarting)
            {
                mission2Restarting = true;
                raidWave = 0;
                CompleteMission("1.2", false);
                resultText.text = "Mission failed. Restarting...";
                Invoke(nameof(RestartMission2), 2f);
            }
        }

        private void RestartMission2()
        {
            mission2Restarting = false;
            enemyWaveManager.ResetWave();
            playerHealth.Initialize(playerHealth.MaxHealth);
            StartMission("1.2");
        }

        private void CompleteMission(string missionId, bool success)
        {
            OnMissionCompleted?.Invoke(missionId, success);
        }
    }
}

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class SaveLoadUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ActFlowController flow;
        [SerializeField] private Button[] slotButtons;
        [SerializeField] private Text[] slotInfoTexts;
        [SerializeField] private Button backButton;
        [SerializeField] private Text headerTitle;

        private bool isSaveMode;

        private void Awake()
        {
            if (backButton)
            {
                backButton.onClick.AddListener(Close);
            }
        }

        public void Open(bool saveMode = false)
        {
            isSaveMode = saveMode;
            if (headerTitle) headerTitle.text = isSaveMode ? "SAVE GAME" : "LOAD GAME";
            if (panel) panel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (panel) panel.SetActive(false);
        }

        public void Refresh()
        {
            var saveSystem = FindAnyObjectByType<SaveSystem>();
            flow ??= FindAnyObjectByType<ActFlowController>();

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;
                string fileName = slot == 1 ? "thelighthouse-save.json" : $"thelighthouse-save-slot{slot}.json";
                string fullPath = Path.Combine(Application.persistentDataPath, fileName);
                bool exists = File.Exists(fullPath);

                if (slotInfoTexts != null && i < slotInfoTexts.Length && slotInfoTexts[i] != null)
                {
                    if (exists)
                    {
                        try
                        {
                            string json = File.ReadAllText(fullPath);
                            var data = JsonUtility.FromJson<GameSaveData>(json);
                            string date = !string.IsNullOrEmpty(data.updatedAt) && DateTime.TryParse(data.updatedAt, out var dt)
                                ? dt.ToLocalTime().ToString("g")
                                : "Unknown Date";
                            slotInfoTexts[i].text = $"SLOT {slot}: {data.act.ToUpperInvariant()} — {data.checkpointId}\n<size=12>{date}</size>";
                        }
                        catch
                        {
                            slotInfoTexts[i].text = $"SLOT {slot}: Corrupted Save";
                        }
                    }
                    else
                    {
                        slotInfoTexts[i].text = $"SLOT {slot}: — EMPTY —";
                    }
                }

                if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
                {
                    slotButtons[i].interactable = isSaveMode || exists;
                    slotButtons[i].onClick.RemoveAllListeners();
                    slotButtons[i].onClick.AddListener(() => OnSlotSelected(slot, exists));
                }
            }
        }

        private void OnSlotSelected(int slot, bool exists)
        {
            var saveSystem = FindAnyObjectByType<SaveSystem>();
            if (!saveSystem) return;

            if (isSaveMode)
            {
                saveSystem.SetCurrentSlot(slot);
                saveSystem.Save();
                Refresh();
            }
            else
            {
                if (exists)
                {
                    saveSystem.SetCurrentSlot(slot);
                    Close();
                    flow ??= FindAnyObjectByType<ActFlowController>();
                    flow?.Continue();
                }
            }
        }
    }
}

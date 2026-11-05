using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class ChapterSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private ActFlowController flow;
        [SerializeField] private Button[] chapterButtons;
        [SerializeField] private Text[] chapterStatusLabels;
        [SerializeField] private Button backButton;

        private static readonly string[] ChapterNames =
        {
            "ACT 1: THE EMPTY KEEPER",
            "ACT 2: THE SINGING CELLAR",
            "ACT 3: THE SHALLOWS",
            "ACT 4: THE LAST POWER",
            "ACT 5: THE LAST LIGHT"
        };

        private void Awake()
        {
            if (backButton)
            {
                backButton.onClick.AddListener(Close);
            }
        }

        public void Open()
        {
            if (!panel) return;
            panel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (panel) panel.SetActive(false);
        }

        public void Refresh()
        {
            flow ??= FindAnyObjectByType<ActFlowController>();
            var saveSystem = FindAnyObjectByType<SaveSystem>();
            int highestUnlocked = saveSystem ? saveSystem.HighestUnlockedAct : 1;

            for (int i = 0; i < 5; i++)
            {
                int actIndex = i + 1;
                bool isUnlocked = actIndex <= highestUnlocked;

                if (chapterButtons != null && i < chapterButtons.Length && chapterButtons[i] != null)
                {
                    chapterButtons[i].interactable = isUnlocked;
                    chapterButtons[i].onClick.RemoveAllListeners();
                    chapterButtons[i].onClick.AddListener(() => SelectChapter(actIndex));
                }

                if (chapterStatusLabels != null && i < chapterStatusLabels.Length && chapterStatusLabels[i] != null)
                {
                    chapterStatusLabels[i].text = isUnlocked ? ChapterNames[i] : $"{ChapterNames[i]} [LOCKED]";
                }
            }
        }

        public void SelectChapter(int actIndex)
        {
            flow ??= FindAnyObjectByType<ActFlowController>();
            if (flow)
            {
                panel.SetActive(false);
                flow.SkipToAct(actIndex);
            }
        }
    }
}

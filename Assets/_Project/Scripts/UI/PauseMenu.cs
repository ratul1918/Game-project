using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace TheLightHouse
{
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] GameObject optionsPanel;
        [SerializeField] SettingsMenuUI settingsMenu;
        [SerializeField] SaveLoadUI saveLoad;
        [SerializeField] JournalUI journal;
        [SerializeField] FirstPersonController player;
        [SerializeField] Interactor interactor;
        [SerializeField] TMP_Text saveStatus;
        bool paused;
        bool previousInteractor;

        void Awake()
        {
            EnsureEventSystem();
            if (panel) panel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(false);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (paused) Resume();
                else Pause();
            }
        }

        void OnDisable()
        {
            if (paused) Resume();
        }

        public void Pause()
        {
            EnsureEventSystem();
            paused = true;
            Time.timeScale = 0f;
            if (panel) panel.SetActive(true);
            player ??= FindAnyObjectByType<FirstPersonController>();
            interactor ??= FindAnyObjectByType<Interactor>();
            player?.SetMovementLocked(true);
            player?.SetLookLocked(true);
            if (interactor)
            {
                previousInteractor = interactor.enabled;
                interactor.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            if (panel) panel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(false);
            if (settingsMenu) settingsMenu.Close();
            if (saveLoad) saveLoad.Close();
            if (!journal || !journal.IsOpen)
            {
                player?.SetMovementLocked(false);
                player?.SetLookLocked(false);
                if (interactor) interactor.enabled = previousInteractor;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OpenJournal() { journal?.Open(JournalUI.Tab.Pages); }
        public void OpenOptions() { if (optionsPanel) optionsPanel.SetActive(true); }
        public void CloseOptions() { if (optionsPanel) optionsPanel.SetActive(false); }
        public void OpenSettings() { if (settingsMenu) settingsMenu.Open(); else OpenOptions(); }
        public void OpenSaveMenu() { if (saveLoad) saveLoad.Open(true); else Save(); }
        public void OpenLoadMenu() { if (saveLoad) saveLoad.Open(false); }

        public void Save()
        {
            FindAnyObjectByType<SaveSystem>()?.Save();
            if (saveStatus)
            {
                saveStatus.text = "Game Saved!";
                saveStatus.gameObject.SetActive(true);
            }
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
        }

        public void EnsureEventSystem()
        {
            if (EventSystem.current == null && FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                var mod = es.GetComponent<InputSystemUIInputModule>();
                mod?.AssignDefaultActions();
            }
        }
    }
}

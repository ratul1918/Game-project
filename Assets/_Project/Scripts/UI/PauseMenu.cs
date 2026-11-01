using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheLightHouse
{
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] GameObject optionsPanel;
        [SerializeField] JournalUI journal;
        [SerializeField] FirstPersonController player;
        [SerializeField] Interactor interactor;
        [SerializeField] TMP_Text saveStatus;
        bool paused;
        bool previousInteractor;

        void Awake(){if(panel)panel.SetActive(false);if(optionsPanel)optionsPanel.SetActive(false);}
        void Update(){if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(paused)Resume();else Pause();}}
        void OnDisable(){if(paused)Resume();}

        public void Pause()
        {
            paused=true;Time.timeScale=0f;if(panel)panel.SetActive(true);player??=FindAnyObjectByType<FirstPersonController>();interactor??=FindAnyObjectByType<Interactor>();
            player?.SetMovementLocked(true);player?.SetLookLocked(true);if(interactor){previousInteractor=interactor.enabled;interactor.enabled=false;}Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        }
        public void Resume()
        {
            paused=false;Time.timeScale=1f;if(panel)panel.SetActive(false);if(optionsPanel)optionsPanel.SetActive(false);
            if(!journal||!journal.IsOpen){player?.SetMovementLocked(false);player?.SetLookLocked(false);if(interactor)interactor.enabled=previousInteractor;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
        }
        public void OpenJournal(){journal?.Open(JournalUI.Tab.Pages);}
        public void OpenOptions(){if(optionsPanel)optionsPanel.SetActive(true);}
        public void Save(){FindAnyObjectByType<SaveSystem>()?.Save();if(saveStatus)saveStatus.text="Saved";}
        public void QuitToMainMenu(){Time.timeScale=1f;SceneManager.LoadScene("MainMenu");}
    }
}

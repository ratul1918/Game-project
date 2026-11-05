using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class MainMenu : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private ActFlowController flow;
        [SerializeField] private GameObject root;
        [SerializeField] private GameOptionsData options = new();
        [SerializeField] private Button continueButton;

        [Header("Submenu Panels")]
        [SerializeField] private ChapterSelectUI chapterSelect;
        [SerializeField] private SaveLoadUI saveLoad;
        [SerializeField] private SettingsMenuUI settingsMenu;
        [SerializeField] private GameObject extrasPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject confirmNewGameModal;

        [Header("Authentication & Network")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private NetworkConfig networkConfig;
        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Text connectionStatus;
        [SerializeField] private Text telemetryNote;

        private void Awake()
        {
            if (telemetryNote)
            {
                telemetryNote.text = "Telemetry collects gameplay events only; no personal data beyond your username.";
            }

            // Ensure GameSettingsManager exists
            if (!GameSettingsManager.Instance)
            {
                var mgrGo = new GameObject("GameSettingsManager");
                mgrGo.AddComponent<GameSettingsManager>();
            }

            CheckContinueState();
        }

        private void OnEnable()
        {
            CheckContinueState();
        }

        private void CheckContinueState()
        {
            var saveSystem = FindAnyObjectByType<SaveSystem>();
            bool hasSave = saveSystem != null && saveSystem.HasSave;
            if (continueButton)
            {
                continueButton.interactable = hasSave;
            }
        }

        public void StartGame()
        {
            var saveSystem = FindAnyObjectByType<SaveSystem>();
            if (saveSystem != null && saveSystem.HasSave)
            {
                if (confirmNewGameModal)
                {
                    confirmNewGameModal.SetActive(true);
                    return;
                }
            }
            ExecuteStartNew();
        }

        public void ConfirmNewGame()
        {
            if (confirmNewGameModal) confirmNewGameModal.SetActive(false);
            ExecuteStartNew();
        }

        public void CancelNewGame()
        {
            if (confirmNewGameModal) confirmNewGameModal.SetActive(false);
        }

        private void ExecuteStartNew()
        {
            root?.SetActive(false);
            flow ??= FindAnyObjectByType<ActFlowController>();
            flow?.StartNew();
        }

        public void ContinueGame()
        {
            root?.SetActive(false);
            flow ??= FindAnyObjectByType<ActFlowController>();
            flow?.Continue();
        }

        public void ShowChapterSelect()
        {
            if (chapterSelect)
            {
                chapterSelect.Open();
            }
        }

        public void ShowLoadGame()
        {
            if (saveLoad)
            {
                saveLoad.Open(false);
            }
        }

        public void ShowSettings()
        {
            if (settingsMenu)
            {
                settingsMenu.Open();
            }
        }

        public void ShowExtras()
        {
            if (extrasPanel) extrasPanel.SetActive(true);
        }

        public void CloseExtras()
        {
            if (extrasPanel) extrasPanel.SetActive(false);
        }

        public void ShowCredits()
        {
            if (creditsPanel) creditsPanel.SetActive(true);
        }

        public void CloseCredits()
        {
            if (creditsPanel) creditsPanel.SetActive(false);
        }

        public void PlayOffline()
        {
            root?.SetActive(false);
            flow ??= FindAnyObjectByType<ActFlowController>();
            flow?.StartNew();
        }

        public void ShowLogin()
        {
            loginPanel?.SetActive(true);
        }

        public void Login()
        {
            Authenticate(false);
        }

        public void Register()
        {
            Authenticate(true);
        }

        private void Authenticate(bool register)
        {
            var auth = FindAnyObjectByType<AuthClient>();
            if (!auth || !networkConfig)
            {
                if (connectionStatus) connectionStatus.text = "Offline";
                return;
            }
            Action<bool> completed = success =>
            {
                if (connectionStatus) connectionStatus.text = success ? "Connected" : "Offline";
            };
            if (register) auth.Register(networkConfig, usernameInput?.text, passwordInput?.text, completed);
            else auth.Login(networkConfig, usernameInput?.text, passwordInput?.text, completed);
        }

        public void SetReducedStorm(bool value)
        {
            options.reducedStorm = value;
            GameSettingsManager.Instance?.UpdateOptions(o => o.reducedStorm = value);
            flow?.ApplyOptions(options);
        }

        public void SetDisableBob(bool value)
        {
            options.disableBobAndFov = value;
            GameSettingsManager.Instance?.UpdateOptions(o => o.disableBobAndFov = value);
            flow?.ApplyOptions(options);
        }

        public void SetSensitivity(float value)
        {
            options.sensitivity = value;
            GameSettingsManager.Instance?.UpdateOptions(o => o.sensitivity = value);
            flow?.ApplyOptions(options);
        }

        public void SetVolume(float value)
        {
            options.volume = value;
            AudioListener.volume = value;
            GameSettingsManager.Instance?.UpdateOptions(o => o.volume = value);
            flow?.ApplyOptions(options);
        }

        public void SetRenderScale(float value)
        {
            options.renderScale = Mathf.Clamp(value, 0.7f, 1f);
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset pipeline)
            {
                pipeline.renderScale = options.renderScale;
            }
            GameSettingsManager.Instance?.UpdateOptions(o => o.renderScale = options.renderScale);
            flow?.ApplyOptions(options);
        }

        public void SetTelemetryEnabled(bool value)
        {
            options.telemetryEnabled = value;
            GameSettingsManager.Instance?.UpdateOptions(o => o.telemetryEnabled = value);
            flow?.ApplyOptions(options);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}

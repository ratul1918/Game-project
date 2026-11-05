using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class SettingsMenuUI : MonoBehaviour
    {
        public enum Tab { Display, Audio, Gameplay, Accessibility, Controls }

        [Header("Panels & Tabs")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject displayTab;
        [SerializeField] private GameObject audioTab;
        [SerializeField] private GameObject gameplayTab;
        [SerializeField] private GameObject accessibilityTab;
        [SerializeField] private GameObject controlsTab;

        [Header("Tab Buttons")]
        [SerializeField] private Button displayTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button gameplayTabButton;
        [SerializeField] private Button accessibilityTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetDefaultsButton;

        [Header("Display Controls")]
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vSyncToggle;
        [SerializeField] private Dropdown targetFpsDropdown;
        [SerializeField] private Slider renderScaleSlider;
        [SerializeField] private Text renderScaleValueText;
        [SerializeField] private Dropdown qualityPresetDropdown;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Text fovValueText;
        [SerializeField] private Slider brightnessSlider;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider dialogueVolumeSlider;
        [SerializeField] private Slider footstepsVolumeSlider;
        [SerializeField] private Slider horrorVolumeSlider;

        [Header("Gameplay Controls")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Text sensitivityValueText;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Toggle cameraBobToggle;
        [SerializeField] private Slider headBobIntensitySlider;
        [SerializeField] private Toggle motionSicknessToggle;
        [SerializeField] private Toggle objectiveAssistanceToggle;
        [SerializeField] private Dropdown difficultyDropdown;

        [Header("Accessibility Controls")]
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Slider subtitleSizeSlider;
        [SerializeField] private Slider subtitleBgOpacitySlider;
        [SerializeField] private Toggle reducedFlashingToggle;
        [SerializeField] private Toggle reducedCameraShakeToggle;
        [SerializeField] private Toggle reducedStormToggle;
        [SerializeField] private Toggle toggleInteractionToggle;

        [Header("Controls Display")]
        [SerializeField] private Text keybindingsText;

        private Resolution[] availableResolutions;
        private Tab currentTab = Tab.Display;

        private void Awake()
        {
            SetupTabButtons();
            PopulateResolutions();
            PopulateTargetFps();
            PopulateQualityPresets();
            PopulateDifficulty();
            SetupListeners();
            if (keybindingsText)
            {
                keybindingsText.text =
                    "MOVEMENT: W, A, S, D\n" +
                    "LOOK: Mouse\n" +
                    "SPRINT: Left Shift\n" +
                    "CROUCH: Left Ctrl / C\n" +
                    "JUMP: Space\n" +
                    "INTERACT: E / Left Mouse Click\n" +
                    "JOURNAL: J\n" +
                    "PAUSE / MENU: Escape\n" +
                    "LEAN LEFT/RIGHT: Q / E (while aiming)\n" +
                    "AXE ATTACK: Left Mouse Button\n" +
                    "AXE BLOCK / SHOVE: Right Mouse Button";
            }
        }

        public void Open()
        {
            if (!rootPanel) return;
            rootPanel.SetActive(true);
            RefreshUIFromSettings();
            SelectTab(Tab.Display);
        }

        public void Close()
        {
            if (rootPanel) rootPanel.SetActive(false);
        }

        private void SetupTabButtons()
        {
            if (displayTabButton) displayTabButton.onClick.AddListener(() => SelectTab(Tab.Display));
            if (audioTabButton) audioTabButton.onClick.AddListener(() => SelectTab(Tab.Audio));
            if (gameplayTabButton) gameplayTabButton.onClick.AddListener(() => SelectTab(Tab.Gameplay));
            if (accessibilityTabButton) accessibilityTabButton.onClick.AddListener(() => SelectTab(Tab.Accessibility));
            if (controlsTabButton) controlsTabButton.onClick.AddListener(() => SelectTab(Tab.Controls));
            if (backButton) backButton.onClick.AddListener(Close);
            if (resetDefaultsButton) resetDefaultsButton.onClick.AddListener(OnResetDefaults);
        }

        public void SelectTab(Tab tab)
        {
            currentTab = tab;
            if (displayTab) displayTab.SetActive(tab == Tab.Display);
            if (audioTab) audioTab.SetActive(tab == Tab.Audio);
            if (gameplayTab) gameplayTab.SetActive(tab == Tab.Gameplay);
            if (accessibilityTab) accessibilityTab.SetActive(tab == Tab.Accessibility);
            if (controlsTab) controlsTab.SetActive(tab == Tab.Controls);
        }

        private void PopulateResolutions()
        {
            if (!resolutionDropdown) return;
            resolutionDropdown.ClearOptions();
            availableResolutions = Screen.resolutions;
            var options = new System.Collections.Generic.List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < availableResolutions.Length; i++)
            {
                var r = availableResolutions[i];
                options.Add($"{r.width} x {r.height} @ {r.refreshRateRatio.value:0}Hz");
                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        private void PopulateTargetFps()
        {
            if (!targetFpsDropdown) return;
            targetFpsDropdown.ClearOptions();
            targetFpsDropdown.AddOptions(new System.Collections.Generic.List<string> { "30 FPS", "60 FPS", "120 FPS", "UNCAPPED" });
        }

        private void PopulateQualityPresets()
        {
            if (!qualityPresetDropdown) return;
            qualityPresetDropdown.ClearOptions();
            qualityPresetDropdown.AddOptions(new System.Collections.Generic.List<string> { "LOW / MAC", "MEDIUM", "HIGH" });
        }

        private void PopulateDifficulty()
        {
            if (!difficultyDropdown) return;
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string> { "STORY (Forgiving)", "SURVIVAL (Standard)", "NIGHTMARE (Punishing)" });
        }

        private void SetupListeners()
        {
            // Display
            if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (vSyncToggle) vSyncToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.vSync = val));
            if (targetFpsDropdown) targetFpsDropdown.onValueChanged.AddListener(OnFpsChanged);
            if (renderScaleSlider) renderScaleSlider.onValueChanged.AddListener(OnRenderScaleChanged);
            if (qualityPresetDropdown) qualityPresetDropdown.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.qualityPreset = val));
            if (fovSlider) fovSlider.onValueChanged.AddListener(OnFovChanged);
            if (brightnessSlider) brightnessSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.brightness = val));

            // Audio
            if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.volume = o.masterVolume = val));
            if (musicVolumeSlider) musicVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.musicVolume = val));
            if (ambienceVolumeSlider) ambienceVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.ambienceVolume = val));
            if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.sfxVolume = val));
            if (dialogueVolumeSlider) dialogueVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.dialogueVolume = val));
            if (footstepsVolumeSlider) footstepsVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.footstepsVolume = val));
            if (horrorVolumeSlider) horrorVolumeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.horrorVolume = val));

            // Gameplay
            if (sensitivitySlider) sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            if (invertYToggle) invertYToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.invertY = val));
            if (cameraBobToggle) cameraBobToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => { o.cameraBob = val; o.disableBobAndFov = !val; }));
            if (headBobIntensitySlider) headBobIntensitySlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.headBobIntensity = val));
            if (motionSicknessToggle) motionSicknessToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.motionSicknessReduction = val));
            if (objectiveAssistanceToggle) objectiveAssistanceToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.objectiveAssistance = val));
            if (difficultyDropdown) difficultyDropdown.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.difficulty = val));

            // Accessibility
            if (subtitlesToggle) subtitlesToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.subtitlesEnabled = val));
            if (subtitleSizeSlider) subtitleSizeSlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.subtitleSize = val));
            if (subtitleBgOpacitySlider) subtitleBgOpacitySlider.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.subtitleBackgroundOpacity = val));
            if (reducedFlashingToggle) reducedFlashingToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.reducedFlashing = val));
            if (reducedCameraShakeToggle) reducedCameraShakeToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.reducedCameraShake = val));
            if (reducedStormToggle) reducedStormToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.reducedStorm = val));
            if (toggleInteractionToggle) toggleInteractionToggle.onValueChanged.AddListener(val => GetSettings()?.UpdateOptions(o => o.toggleInteraction = val));
        }

        public void RefreshUIFromSettings()
        {
            var mgr = GetSettings();
            var o = mgr != null ? mgr.CurrentOptions : new GameOptionsData();

            if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;
            if (vSyncToggle) vSyncToggle.isOn = o.vSync;
            if (renderScaleSlider) { renderScaleSlider.value = o.renderScale; if (renderScaleValueText) renderScaleValueText.text = $"{Mathf.RoundToInt(o.renderScale * 100)}%"; }
            if (qualityPresetDropdown) qualityPresetDropdown.value = Mathf.Clamp(o.qualityPreset, 0, 2);
            if (fovSlider) { fovSlider.value = o.fov; if (fovValueText) fovValueText.text = $"{Mathf.RoundToInt(o.fov)}°"; }
            if (brightnessSlider) brightnessSlider.value = o.brightness;

            if (masterVolumeSlider) masterVolumeSlider.value = o.volume;
            if (musicVolumeSlider) musicVolumeSlider.value = o.musicVolume;
            if (ambienceVolumeSlider) ambienceVolumeSlider.value = o.ambienceVolume;
            if (sfxVolumeSlider) sfxVolumeSlider.value = o.sfxVolume;
            if (dialogueVolumeSlider) dialogueVolumeSlider.value = o.dialogueVolume;
            if (footstepsVolumeSlider) footstepsVolumeSlider.value = o.footstepsVolume;
            if (horrorVolumeSlider) horrorVolumeSlider.value = o.horrorVolume;

            if (sensitivitySlider) { sensitivitySlider.value = o.sensitivity; if (sensitivityValueText) sensitivityValueText.text = $"{o.sensitivity:0.0}x"; }
            if (invertYToggle) invertYToggle.isOn = o.invertY;
            if (cameraBobToggle) cameraBobToggle.isOn = o.cameraBob;
            if (headBobIntensitySlider) headBobIntensitySlider.value = o.headBobIntensity;
            if (motionSicknessToggle) motionSicknessToggle.isOn = o.motionSicknessReduction;
            if (objectiveAssistanceToggle) objectiveAssistanceToggle.isOn = o.objectiveAssistance;
            if (difficultyDropdown) difficultyDropdown.value = Mathf.Clamp(o.difficulty, 0, 2);

            if (subtitlesToggle) subtitlesToggle.isOn = o.subtitlesEnabled;
            if (subtitleSizeSlider) subtitleSizeSlider.value = o.subtitleSize;
            if (subtitleBgOpacitySlider) subtitleBgOpacitySlider.value = o.subtitleBackgroundOpacity;
            if (reducedFlashingToggle) reducedFlashingToggle.isOn = o.reducedFlashing;
            if (reducedCameraShakeToggle) reducedCameraShakeToggle.isOn = o.reducedCameraShake;
            if (reducedStormToggle) reducedStormToggle.isOn = o.reducedStorm;
            if (toggleInteractionToggle) toggleInteractionToggle.isOn = o.toggleInteraction;
        }

        private void OnResolutionChanged(int index)
        {
            if (availableResolutions == null || index < 0 || index >= availableResolutions.Length) return;
            var r = availableResolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void OnFpsChanged(int index)
        {
            int fps = index switch { 0 => 30, 1 => 60, 2 => 120, _ => -1 };
            GetSettings()?.UpdateOptions(o => o.frameRateLimit = fps);
        }

        private void OnRenderScaleChanged(float val)
        {
            if (renderScaleValueText) renderScaleValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
            GetSettings()?.UpdateOptions(o => o.renderScale = val);
        }

        private void OnFovChanged(float val)
        {
            if (fovValueText) fovValueText.text = $"{Mathf.RoundToInt(val)}°";
            GetSettings()?.UpdateOptions(o => o.fov = val);
        }

        private void OnSensitivityChanged(float val)
        {
            if (sensitivityValueText) sensitivityValueText.text = $"{val:0.0}x";
            GetSettings()?.UpdateOptions(o => o.sensitivity = val);
        }

        private void OnResetDefaults()
        {
            GetSettings()?.ResetToDefaults();
            RefreshUIFromSettings();
        }

        private GameSettingsManager GetSettings()
        {
            var mgr = GameSettingsManager.Instance;
            if (!mgr)
            {
                var go = new GameObject("GameSettingsManager");
                mgr = go.AddComponent<GameSettingsManager>();
            }
            return mgr;
        }
    }
}

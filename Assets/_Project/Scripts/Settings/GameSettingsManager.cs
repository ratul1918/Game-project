using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLightHouse
{
    public sealed class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Instance { get; private set; }

        [SerializeField] private GameOptionsData currentOptions = new();

        public GameOptionsData CurrentOptions => currentOptions;

        public event Action<GameOptionsData> OptionsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPrefs();
            ApplyAll(currentOptions);
        }

        public void UpdateOptions(Action<GameOptionsData> mutator)
        {
            if (mutator == null) return;
            mutator(currentOptions);
            ApplyAll(currentOptions);
            SaveToPrefs();
            OptionsChanged?.Invoke(currentOptions);
        }

        public void SetOptions(GameOptionsData newOptions)
        {
            if (newOptions == null) return;
            currentOptions = newOptions;
            ApplyAll(currentOptions);
            SaveToPrefs();
            OptionsChanged?.Invoke(currentOptions);
        }

        public void ApplyAll(GameOptionsData options)
        {
            if (options == null) return;

            // Audio
            AudioListener.volume = Mathf.Clamp01(options.volume);

            // Display & Graphics
            if (options.qualityPreset >= 0 && options.qualityPreset < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(options.qualityPreset, true);
            }

            QualitySettings.vSyncCount = options.vSync ? 1 : 0;
            Application.targetFrameRate = options.frameRateLimit > 0 ? options.frameRateLimit : -1;

            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.renderScale = Mathf.Clamp(options.renderScale, 0.7f, 1.0f);
            }

            // Player / Camera options
            var controller = FindAnyObjectByType<FirstPersonController>();
            if (controller)
            {
                controller.ApplyOptions(options.sensitivity, options.disableBobAndFov);
            }

            // Accessibility
            var accessibility = FindAnyObjectByType<AccessibilityOptions>();
            if (accessibility)
            {
                accessibility.SetReduced(options.reducedStorm);
            }

            var subtitles = FindAnyObjectByType<SubtitleUI>();
            if (subtitles)
            {
                var hudSettings = ScriptableObject.CreateInstance<HudSettings>();
                hudSettings.SetSubtitleBackground(options.subtitleBackgroundOpacity);
                hudSettings.SetSubtitleScale(options.subtitleSize);
                subtitles.ApplySettings(hudSettings);
            }
        }

        public void ResetToDefaults()
        {
            currentOptions = new GameOptionsData();
            ApplyAll(currentOptions);
            SaveToPrefs();
            OptionsChanged?.Invoke(currentOptions);
        }

        private void SaveToPrefs()
        {
            string json = JsonUtility.ToJson(currentOptions);
            PlayerPrefs.SetString("TheLightHouse_Settings", json);
            PlayerPrefs.Save();
        }

        private void LoadFromPrefs()
        {
            if (PlayerPrefs.HasKey("TheLightHouse_Settings"))
            {
                string json = PlayerPrefs.GetString("TheLightHouse_Settings");
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonUtility.FromJson<GameOptionsData>(json);
                    if (loaded != null)
                    {
                        currentOptions = loaded;
                    }
                }
            }
        }
    }
}

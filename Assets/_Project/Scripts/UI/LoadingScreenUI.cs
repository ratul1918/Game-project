using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class LoadingScreenUI : MonoBehaviour
    {
        public static LoadingScreenUI Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text tipText;
        [SerializeField] private Image progressBar;

        private static readonly string[] Tips =
        {
            "Pale figures in the shallows freeze while observed. Looking away allows them to move closer.",
            "Cranking the emergency lantern produces heavy noise. Time your strokes between thunder rolls.",
            "Wet footprints in the cellar lead inwards, but never back out.",
            "Keep the Fresnel lamp fueled. Complete darkness releases the ocean's grasp.",
            "The stalker investigates sound. Sneak and crouch on creaking floorboards to avoid detection.",
            "The maritime rescue axe can stagger the Drowned, creating temporary escape opportunities."
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (panel) panel.SetActive(false);
            if (canvasGroup) canvasGroup.alpha = 0f;
        }

        public void Show(float progress = 0f)
        {
            if (!panel) return;
            panel.SetActive(true);
            if (tipText)
            {
                int idx = UnityEngine.Random.Range(0, Tips.Length);
                tipText.text = Tips[idx];
            }
            if (progressBar) progressBar.fillAmount = progress;
            if (canvasGroup) canvasGroup.alpha = 1f;
        }

        public void UpdateProgress(float progress)
        {
            if (progressBar) progressBar.fillAmount = Mathf.Clamp01(progress);
        }

        public void Hide()
        {
            if (!panel) return;
            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            const float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            if (canvasGroup) canvasGroup.alpha = 0f;
            if (panel) panel.SetActive(false);
        }
    }
}

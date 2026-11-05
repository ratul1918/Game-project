using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class DeathScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text quoteText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SaveLoadUI saveLoad;

        private static readonly string[] DeathQuotes =
        {
            "\"The sea took them all, and the sea never forgets.\"",
            "\"Never answer the singing from the cellar.\"",
            "\"Keep the light. Darkness lets them advance.\"",
            "\"Six sailors drowned in the gale. You were the only one who reached the shore.\"",
            "\"The lighthouse was never a guide. It was a leash.\""
        };

        private void Awake()
        {
            if (panel) panel.SetActive(false);
            if (canvasGroup) canvasGroup.alpha = 0f;

            if (retryButton) retryButton.onClick.AddListener(OnRetry);
            if (loadButton) loadButton.onClick.AddListener(OnLoad);
            if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        public void ShowDeath()
        {
            if (!panel) return;
            panel.SetActive(true);
            if (quoteText)
            {
                int idx = UnityEngine.Random.Range(0, DeathQuotes.Length);
                quoteText.text = DeathQuotes[idx];
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StopAllCoroutines();
            StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            float elapsed = 0f;
            const float duration = 1.5f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (canvasGroup) canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            if (canvasGroup) canvasGroup.alpha = 1f;
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;
            if (panel) panel.SetActive(false);
            var health = FindAnyObjectByType<PlayerHealthState>();
            if (health)
            {
                health.Respawn();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnLoad()
        {
            if (saveLoad)
            {
                saveLoad.Open(false);
            }
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}

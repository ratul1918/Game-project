using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SubtitleUI : MonoBehaviour
    {
        public static SubtitleUI Instance { get; private set; }

        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField, Min(0f)] private float horizontalPadding = 48f;
        [SerializeField, Min(0f)] private float verticalPadding = 18f;
        [SerializeField, Min(0.1f)] private float fallbackDuration = 4f;

        private readonly Queue<DialogueLine> queue = new();
        private bool playing;
        private Coroutine autoHideCoroutine;

        public bool Busy => queue.Count > 0 || playing || autoHideCoroutine != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureRaycastTargetOff();
            Hide();
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            EnsureRaycastTargetOff();
            GameEvents.DialogueRequested += Enqueue;
        }

        private void OnDisable()
        {
            GameEvents.DialogueRequested -= Enqueue;
            StopAllCoroutines();
            queue.Clear();
            playing = false;
            autoHideCoroutine = null;
            if (Instance == this) Instance = null;
        }

        public void ApplySettings(HudSettings settings)
        {
            if (!settings) return;
            if (label) label.fontSize = Mathf.RoundToInt(26f * settings.SubtitleScale);
            if (background)
            {
                Color color = background.color;
                color.a = settings.SubtitleBackgroundOpacity;
                background.color = color;
            }
            EnsureRaycastTargetOff();
            Hide();
        }

        private void EnsureRaycastTargetOff()
        {
            if (label != null) label.raycastTarget = false;
            if (background != null) background.raycastTarget = false;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var bg = canvas.transform.Find("Subtitle Background");
                if (bg != null && bg.TryGetComponent<Image>(out var bgImg))
                {
                    bgImg.raycastTarget = false;
                }
            }

            // Also ensure any graphics on the same or parent canvas overlay do not absorb mouse/keyboard raycasts
            var allGraphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in allGraphics)
            {
                if (g != null && !(g is Button) && !(g is Toggle) && !(g is Slider))
                {
                    g.raycastTarget = false;
                }
            }
        }

        private void Enqueue(DialogueLine line)
        {
            // If line is handled directly by VoiceManager or Boatman runner, queue handles fallback
            queue.Enqueue(line);
            if (!playing) StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            playing = true;
            while (queue.Count > 0)
            {
                var line = queue.Dequeue();
                Show(line.speaker, line.text);
                if (line.clip && TheLightHouse.Voice.VoiceManager.Instance == null)
                {
                    var src = GetComponent<AudioSource>();
                    if (src) src.PlayOneShot(line.clip);
                }
                float dur = line.clip != null ? line.clip.length : (line.duration > 0 ? line.duration : fallbackDuration);
                yield return new WaitForSeconds(dur + 0.4f);
            }
            Hide();
            playing = false;
        }

        public void Show(DialogueLine line)
        {
            if (line == null) return;
            float dur = line.clip != null ? line.clip.length + 0.5f : Mathf.Max(2.5f, line.text.Length * 0.06f);
            Show(line.speaker, line.text, dur);
        }

        public void Show(string speaker, string text, float duration = 0f)
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

            if (!label) label = GetComponent<Text>();
            if (!label) return;

            EnsureRaycastTargetOff();
            label.text = string.IsNullOrEmpty(speaker) ? text : $"{speaker}: {text}";
            label.enabled = true;

            RectTransform labelRect = label.rectTransform;
            Vector2 size = new(
                Mathf.Max(1f, label.preferredWidth) + horizontalPadding,
                Mathf.Max(1f, label.preferredHeight) + verticalPadding
            );
            labelRect.sizeDelta = size;

            if (background)
            {
                background.enabled = true;
                background.rectTransform.sizeDelta = size;
                background.raycastTarget = false;
            }

            if (duration > 0f && gameObject.activeInHierarchy)
            {
                autoHideCoroutine = StartCoroutine(AutoHideRoutine(duration));
            }
        }

        private IEnumerator AutoHideRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
            autoHideCoroutine = null;
        }

        public void Hide()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

            if (label)
            {
                label.text = "";
                label.enabled = false;
            }
            if (background)
            {
                background.enabled = false;
            }
        }
    }
}

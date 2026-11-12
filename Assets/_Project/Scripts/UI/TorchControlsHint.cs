using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class TorchControlsHint : MonoBehaviour
    {
        [SerializeField] private Vector2 hintSize = new Vector2(350f, 30f);
        [SerializeField] private Vector2 margin = new Vector2(-24f, 24f);
        private GameObject _canvas;
        private bool _reading;
        private void Awake()
        {
            _canvas = new GameObject("Torch controls", typeof(Canvas), typeof(CanvasScaler));
            _canvas.transform.SetParent(transform, false);
            var canvas = _canvas.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = .5f;
            var label = new GameObject("Hint", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(_canvas.transform, false);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = hintSize; rect.anchoredPosition = margin;
            var text = label.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18; text.alignment = TextAnchor.MiddleRight; text.raycastTarget = false;
            text.color = new Color(.88f, .87f, .8f);
            text.text = "[1] Equip Torch | [F] Toggle Light";
            label.AddComponent<Shadow>().effectDistance = new Vector2(1f, -1f);
        }
        private void OnEnable() => GameEvents.ReadingStateChanged += Reading;
        private void OnDisable() { GameEvents.ReadingStateChanged -= Reading; if (_canvas) _canvas.SetActive(false); }
        private void Reading(ReadingStateChangedEvent e) => _reading = e.IsReading;
        private void Update() => _canvas.SetActive(!_reading && Time.timeScale > 0f && Cursor.lockState == CursorLockMode.Locked);
    }
}

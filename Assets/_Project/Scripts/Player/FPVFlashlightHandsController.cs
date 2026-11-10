using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    [DisallowMultipleComponent]
    public sealed class FPVFlashlightHandsController : MonoBehaviour
    {
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private bool isEquipped = true;
        [SerializeField] private bool isOn = true;
        [SerializeField] private Light spotLight;
        [SerializeField] private Light fillLight;
        [SerializeField] private Renderer bulbFilamentRenderer;
        [SerializeField] private Material bulbMaterial;
        [SerializeField] private float spotIntensity = 24f;
        [SerializeField] private float fillIntensity = .85f;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource humSource;
        [SerializeField] private AudioClip switchOnClip;
        [SerializeField] private AudioClip switchOffClip;
        [SerializeField] private AudioClip clothRustleClip;
        [SerializeField] private AudioClip filamentHumClip;
        [SerializeField] private AudioClip flickerSparksClip;
        [SerializeField] private Transform armRootTransform;
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField, Min(.1f)] private float equipDuration = .45f;
        private TorchViewmodel _viewmodel;
        private Coroutine _transition;
        private Coroutine _flickerRoutine;
        private bool _switchOn;
        private bool _reading;
        private bool _started;
        private bool _enabledToggleAction;
        public bool IsEquipped => isEquipped;
        public bool IsOn => isOn;
        public bool IsTransitioning => _transition != null;

        public void Initialize(Animator animator, AudioSource sfxAudio, AudioSource humAudio,
            Light spot, Light fill, Renderer bulbRenderer, Material bulbMat,
            Transform armRootTransform, Transform flashlightRoot)
        {
            handsAnimator = animator; sfxSource = sfxAudio; humSource = humAudio;
            spotLight = spot; fillLight = fill; bulbFilamentRenderer = bulbRenderer;
            bulbMaterial = bulbMat; this.armRootTransform = armRootTransform;
        }

        private void Start()
        {
            if (handsAnimator) handsAnimator.enabled = false;
            _switchOn = isOn;
            if (!switchOnClip) switchOnClip = Resources.Load<AudioClip>("TorchAudio/Flashlight_Switch_On");
            if (!switchOffClip) switchOffClip = Resources.Load<AudioClip>("TorchAudio/Flashlight_Switch_Off");
            if (!clothRustleClip) clothRustleClip = Resources.Load<AudioClip>("TorchAudio/Coat_Rustle_WetWool");
            if (!filamentHumClip) filamentHumClip = Resources.Load<AudioClip>("TorchAudio/Flashlight_Filament_Hum");
            if (!flickerSparksClip) flickerSparksClip = Resources.Load<AudioClip>("TorchAudio/Flashlight_Flicker_Sparks");
            if (humSource) humSource.clip = filamentHumClip;
            _viewmodel = gameObject.AddComponent<TorchViewmodel>();
            _viewmodel.Initialize(armRootTransform);
            _viewmodel.DrawAmount = isEquipped ? 1f : 0f;
            gameObject.AddComponent<TorchControlsHint>();
            if (bulbFilamentRenderer) bulbMaterial = bulbFilamentRenderer.material;
            int vmLayer = LayerMask.NameToLayer("Viewmodel");
            if (vmLayer < 0) vmLayer = 2;
            if (spotLight)
            {
                spotLight.cullingMask &= ~(1 << vmLayer);
                spotLight.renderingLayerMask = 1;
                spotLight.shadowNearPlane = 0.05f;
            }
            if (fillLight)
            {
                fillLight.cullingMask &= ~(1 << vmLayer);
                fillLight.renderingLayerMask = 1;
                fillLight.intensity = 0f;
                fillLight.enabled = false;
            }
            // Old scenes can contain a second light pair created by the compatibility component.
            foreach (var light in GetComponentsInChildren<Light>(true))
                if (light != spotLight && light != fillLight) light.enabled = false;
            ApplyLight(isEquipped && _switchOn, false);
            _started = true;
            GameEvents.RaiseTorchEquippedChanged(isEquipped);
            GameEvents.RaiseItemCollected(new ItemCollectedEvent("torch"));
        }

        private void OnEnable()
        {
            GameEvents.ReadingStateChanged += OnReading;
            if (toggleAction != null && toggleAction.action != null && !toggleAction.action.enabled)
            {
                toggleAction.action.Enable();
                _enabledToggleAction = true;
            }
            if (_viewmodel) _viewmodel.enabled = true;
            if (_started) ApplyLight(isEquipped && _switchOn, false);
        }

        private void OnDisable()
        {
            GameEvents.ReadingStateChanged -= OnReading;
            if (_enabledToggleAction) toggleAction.action.Disable();
            _enabledToggleAction = false;
            if (_viewmodel) _viewmodel.enabled = false;
            if (_transition != null) StopCoroutine(_transition);
            _transition = null;
            if (_viewmodel) _viewmodel.DrawAmount = isEquipped ? 1f : 0f;
            ApplyLight(false, false);
        }

        private void OnReading(ReadingStateChangedEvent e) => _reading = e.IsReading;
        private bool CanInput => Time.timeScale > 0f && !_reading && (Cursor.lockState == CursorLockMode.Locked || Application.isEditor);

        private void Update()
        {
            if (!CanInput) return;
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                if (isEquipped) Unequip(); else Equip();
                return;
            }
            bool pressed = (toggleAction != null && toggleAction.action != null && toggleAction.action.WasPressedThisFrame())
                || (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame);
            if (pressed) Toggle();
        }

        public void Toggle()
        {
            if (!isActiveAndEnabled || !isEquipped || IsTransitioning || !CanInput) return;
            SetLight(!_switchOn);
        }

        public void SetLight(bool on)
        {
            if (!isActiveAndEnabled || !isEquipped || IsTransitioning) return;
            _switchOn = on;
            ApplyLight(on, true);
        }

        public void Equip()
        {
            if (!isActiveAndEnabled || !_started || isEquipped || IsTransitioning) return;
            _transition = StartCoroutine(AnimatePocket(true));
        }

        public void Unequip()
        {
            if (!isActiveAndEnabled || !_started || !isEquipped || IsTransitioning) return;
            isEquipped = false;
            ApplyLight(false, false);
            GameEvents.RaiseTorchEquippedChanged(false);
            _transition = StartCoroutine(AnimatePocket(false));
        }

        private IEnumerator AnimatePocket(bool drawing)
        {
            PlayClothRustle();
            float from = _viewmodel.DrawAmount;
            for (float elapsed = 0f; elapsed < equipDuration; elapsed += Time.deltaTime)
            {
                _viewmodel.DrawAmount = Mathf.Lerp(from, drawing ? 1f : 0f,
                    Mathf.SmoothStep(0f, 1f, elapsed / equipDuration));
                yield return null;
            }
            _viewmodel.DrawAmount = drawing ? 1f : 0f;
            isEquipped = drawing;
            _transition = null;
            if (drawing)
            {
                ApplyLight(_switchOn, false);
                GameEvents.RaiseTorchEquippedChanged(true);
            }
        }

        // Compatibility for older animation events; procedural motion owns completion now.
        public void TurnOnLight() { if (!IsTransitioning) SetLight(true); }
        public void DisableTorchMesh() { if (!isEquipped) ApplyLight(false, false); }
        public void PlayClothRustle()
        {
            if (sfxSource && clothRustleClip) sfxSource.PlayOneShot(clothRustleClip, .75f);
        }

        private void ApplyLight(bool on, bool audio)
        {
            if (_flickerRoutine != null) StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
            bool changed = isOn != on;
            isOn = on;
            SetIntensity(on ? 1f : 0f);
            if (humSource)
            {
                if (on && humSource.clip && !humSource.isPlaying) humSource.Play();
                if (!on) humSource.Stop();
            }
            var clip = on ? switchOnClip : switchOffClip;
            if (changed && audio && clip && sfxSource) sfxSource.PlayOneShot(clip);
            if (changed || !_started) GameEvents.RaiseFlashlightToggled(on);
        }

        private void SetIntensity(float level)
        {
            if (spotLight) { spotLight.enabled = level > 0f; spotLight.intensity = spotIntensity * level; }
            if (fillLight) { fillLight.enabled = false; fillLight.intensity = 0f; }
            if (bulbMaterial)
            {
                bulbMaterial.EnableKeyword("_EMISSION");
                bulbMaterial.SetColor("_EmissionColor", new Color(1f, .88f, .65f) * (level * 3.8f));
            }
        }

        public void TriggerFlicker(float duration = 1.4f, int flickerCount = 7)
        {
            if (!isActiveAndEnabled || !isEquipped || !isOn || IsTransitioning) return;
            if (_flickerRoutine != null) StopCoroutine(_flickerRoutine);
            _flickerRoutine = StartCoroutine(Flicker(duration, flickerCount));
        }

        private IEnumerator Flicker(float duration, int count)
        {
            if (sfxSource && flickerSparksClip) sfxSource.PlayOneShot(flickerSparksClip, .85f);
            float step = Mathf.Max(.01f, duration / Mathf.Max(2, count * 2));
            for (float elapsed = 0; elapsed < duration; elapsed += step)
            {
                SetIntensity(Random.value > .45f ? Random.Range(.25f, 1.15f) : 0f);
                yield return new WaitForSeconds(step);
            }
            SetIntensity(isEquipped && isOn ? 1f : 0f);
            _flickerRoutine = null;
        }
    }
}

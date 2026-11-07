using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    /// <summary>
    /// Operates Daniel Hale's First-Person View (FPV) animated arms and pocket equip/holster system.
    /// Manages the FPV_Flashlight_Hands animator state machine, audio synchronization (cloth rustle,
    /// heavy tactile switch click, filament hum), procedural weapon sway/lag, and horror electrical flicker.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FPVFlashlightHandsController : MonoBehaviour
    {
        [Header("Animator & State")]
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private bool isEquipped = true;
        [SerializeField] private bool isOn = true;

        [Header("Lighting Components")]
        [SerializeField] private Light spotLight;
        [SerializeField] private Light fillLight;
        [SerializeField] private Renderer bulbFilamentRenderer;
        [SerializeField] private Material bulbMaterial;
        [SerializeField] private float spotIntensity = 24f;
        [SerializeField] private float fillIntensity = 0.85f;

        [Header("Audio FX")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource humSource;
        [SerializeField] private AudioClip switchOnClip;
        [SerializeField] private AudioClip switchOffClip;
        [SerializeField] private AudioClip clothRustleClip;
        [SerializeField] private AudioClip filamentHumClip;
        [SerializeField] private AudioClip flickerSparksClip;

        [Header("Sway & Lag")]
        [SerializeField] private Transform armRootTransform;
        [SerializeField] private float swayAmount = 2.4f;
        [SerializeField] private float maxSwayAngle = 6.0f;
        [SerializeField] private float swaySmoothSpeed = 8.5f;

        [Header("Bobbing")]
        [SerializeField] private float idleBobSpeed = 1.4f;
        [SerializeField] private float idleBobAmount = 0.0035f;
        [SerializeField] private float walkBobSpeed = 7.5f;
        [SerializeField] private float walkBobAmount = 0.012f;

        [Header("Input Action")]
        [SerializeField] private InputActionReference toggleAction;

        private Transform _parentCamera;
        private Quaternion _lastCamRot;
        private Quaternion _swayRot;
        private Vector3 _initialArmLocalPos;
        private Quaternion _initialArmLocalRot;
        private float _bobTimer;
        private bool _isTransitioning;
        private Coroutine _flickerRoutine;
        private Transform _flashlightRoot;
        private FirstPersonController _fpc;

        private static readonly int IsEquippedHash = Animator.StringToHash("isEquipped");

        public bool IsEquipped => isEquipped;
        public bool IsOn => isOn;
        public bool IsTransitioning => _isTransitioning;

        public void Initialize(
            Animator animator,
            AudioSource sfxAudio,
            AudioSource humAudio,
            Light spot,
            Light fill,
            Renderer bulbRenderer,
            Material bulbMat,
            Transform armRootTransform,
            Transform flashlightRoot)
        {
            handsAnimator = animator;
            sfxSource = sfxAudio;
            humSource = humAudio;
            spotLight = spot;
            fillLight = fill;
            bulbFilamentRenderer = bulbRenderer;
            bulbMaterial = bulbMat;
            this.armRootTransform = armRootTransform;
            _flashlightRoot = flashlightRoot;

            LoadAudioClipsIfMissing();
        }

        private void Awake()
        {
            _parentCamera = transform.parent;
            if (_parentCamera) _lastCamRot = _parentCamera.rotation;

            if (armRootTransform)
            {
                _initialArmLocalPos = armRootTransform.localPosition;
                _initialArmLocalRot = armRootTransform.localRotation;
            }
            _swayRot = Quaternion.identity;

            _fpc = GetComponentInParent<FirstPersonController>();
            LoadAudioClipsIfMissing();
        }

        private void Start()
        {
            // Start in equipped and illuminated state for seamless narrative immersion
            if (isEquipped)
            {
                if (handsAnimator) handsAnimator.SetBool(IsEquippedHash, true);
                TurnOnLight();
            }
            else
            {
                if (handsAnimator) handsAnimator.SetBool(IsEquippedHash, false);
                DisableTorchMesh();
            }
        }

        private void LoadAudioClipsIfMissing()
        {
            if (!switchOnClip) switchOnClip = LoadAudio("Flashlight_Switch_On.wav");
            if (!switchOffClip) switchOffClip = LoadAudio("Flashlight_Switch_Off.wav");
            if (!clothRustleClip) clothRustleClip = LoadAudio("Coat_Rustle_WetWool.wav");
            if (!filamentHumClip) filamentHumClip = LoadAudio("Flashlight_Filament_Hum.wav");
            if (!flickerSparksClip) flickerSparksClip = LoadAudio("Flashlight_Flicker_Sparks.wav");

            if (humSource && filamentHumClip)
            {
                humSource.clip = filamentHumClip;
            }
        }

        private static AudioClip LoadAudio(string filename)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/_Project/Audio/{filename}");
#else
            return null;
#endif
        }

        private void OnEnable()
        {
            if (toggleAction != null && toggleAction.action != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += OnToggleInput;
            }
        }

        private void OnDisable()
        {
            if (toggleAction != null && toggleAction.action != null)
            {
                toggleAction.action.performed -= OnToggleInput;
                toggleAction.action.Disable();
            }

            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }
        }

        private void OnToggleInput(InputAction.CallbackContext ctx)
        {
            Toggle();
        }

        private void Update()
        {
            // Keyboard 'F' key and Gamepad D-pad Up fallback checks via New Input System
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                Toggle();
            }
            else if (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                Toggle();
            }

            if (isEquipped && !_isTransitioning)
            {
                UpdateBobbing(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (isEquipped && !_isTransitioning)
            {
                UpdateSway(Time.deltaTime);
            }
        }

        public void Toggle()
        {
            if (_isTransitioning) return; // Anti-spam prevention during active transitions

            if (isEquipped)
            {
                Unequip();
            }
            else
            {
                Equip();
            }
        }

        public void Equip()
        {
            if (_isTransitioning) return;

            isEquipped = true;
            _isTransitioning = true;

            if (_flashlightRoot) _flashlightRoot.gameObject.SetActive(true);

            if (handsAnimator)
            {
                handsAnimator.SetBool(IsEquippedHash, true);
            }
            else
            {
                // Fallback if animator is not bound
                TurnOnLight();
                _isTransitioning = false;
            }
        }

        public void Unequip()
        {
            if (_isTransitioning) return;

            isEquipped = false;
            _isTransitioning = true;

            // Cut light and click off immediately when thumb starts pulling back
            CutLightOff();

            PlayClothRustle();

            if (handsAnimator)
            {
                handsAnimator.SetBool(IsEquippedHash, false);
            }
            else
            {
                DisableTorchMesh();
            }
        }

        // =========================================================================
        // Animation Event Callbacks
        // =========================================================================

        /// <summary>
        /// Fired by Draw_From_Pocket animation event at keyframe 0.45s as the torch levels with crosshair.
        /// </summary>
        public void TurnOnLight()
        {
            isOn = true;
            _isTransitioning = false;

            if (spotLight) spotLight.enabled = true;
            if (fillLight) fillLight.enabled = true;

            UpdateBulbEmission(1.0f);

            if (sfxSource && switchOnClip)
            {
                sfxSource.PlayOneShot(switchOnClip, 0.95f);
            }

            if (humSource && filamentHumClip && !humSource.isPlaying)
            {
                humSource.Play();
            }

            GameEvents.RaiseFlashlightToggled(true);
        }

        /// <summary>
        /// Cuts the spotlight beam, ambient fill, and hum off immediately.
        /// </summary>
        private void CutLightOff()
        {
            isOn = false;

            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }

            if (spotLight) spotLight.enabled = false;
            if (fillLight) fillLight.enabled = false;

            UpdateBulbEmission(0.0f);

            if (sfxSource && switchOffClip)
            {
                sfxSource.PlayOneShot(switchOffClip, 0.95f);
            }

            if (humSource && humSource.isPlaying)
            {
                humSource.Stop();
            }

            GameEvents.RaiseFlashlightToggled(false);
        }

        /// <summary>
        /// Fired by Put_Into_Pocket animation event at keyframe 0.48s when the arm is out of view.
        /// </summary>
        public void DisableTorchMesh()
        {
            _isTransitioning = false;
            CutLightOff();
        }

        /// <summary>
        /// Fired by Draw_From_Pocket and Put_Into_Pocket as heavy wool & oilskin sleeves slide into motion.
        /// </summary>
        public void PlayClothRustle()
        {
            if (sfxSource && clothRustleClip)
            {
                sfxSource.PlayOneShot(clothRustleClip, 0.75f);
            }
        }

        // =========================================================================
        // Sway, Lag & Bobbing
        // =========================================================================

        private void UpdateSway(float dt)
        {
            if (!_parentCamera || !armRootTransform) return;

            Quaternion camRot = _parentCamera.rotation;
            Quaternion deltaRot = Quaternion.Inverse(_lastCamRot) * camRot;
            _lastCamRot = camRot;

            float yawDelta = Mathf.DeltaAngle(0f, deltaRot.eulerAngles.y);
            float pitchDelta = Mathf.DeltaAngle(0f, deltaRot.eulerAngles.x);

            float targetSwayY = Mathf.Clamp(-yawDelta * swayAmount, -maxSwayAngle, maxSwayAngle);
            float targetSwayX = Mathf.Clamp(pitchDelta * swayAmount, -maxSwayAngle, maxSwayAngle);

            Quaternion targetSway = Quaternion.Euler(targetSwayX, targetSwayY, -targetSwayY * 0.4f);
            _swayRot = Quaternion.Slerp(_swayRot, targetSway, dt * swaySmoothSpeed);

            armRootTransform.localRotation = _initialArmLocalRot * _swayRot;
        }

        private void UpdateBobbing(float dt)
        {
            if (!armRootTransform) return;

            float speed = _fpc != null ? _fpc.Speed : 0f;
            bool isMoving = speed > 0.1f;

            float currentBobSpeed = isMoving ? walkBobSpeed : idleBobSpeed;
            float currentBobAmount = isMoving ? walkBobAmount : idleBobAmount;

            _bobTimer += dt * currentBobSpeed;

            float bobY = Mathf.Sin(_bobTimer) * currentBobAmount;
            float bobX = Mathf.Cos(_bobTimer * 0.5f) * (currentBobAmount * 0.5f);

            armRootTransform.localPosition = _initialArmLocalPos + new Vector3(bobX, bobY, 0f);
        }

        // =========================================================================
        // Horror Flicker Routine
        // =========================================================================

        public void TriggerFlicker(float duration = 1.4f, int flickerCount = 7)
        {
            if (!isEquipped || !isOn || !gameObject.activeInHierarchy || !enabled) return;

            if (_flickerRoutine != null) StopCoroutine(_flickerRoutine);
            _flickerRoutine = StartCoroutine(FlickerRoutine(duration, flickerCount));
        }

        private IEnumerator FlickerRoutine(float duration, int flickerCount)
        {
            if (flickerSparksClip && sfxSource)
            {
                sfxSource.PlayOneShot(flickerSparksClip, 0.85f);
            }

            float elapsed = 0f;
            float step = duration / Mathf.Max(2, flickerCount * 2);

            while (elapsed < duration)
            {
                bool lightState = Random.value > 0.45f;
                float intensityMult = lightState ? Random.Range(0.25f, 1.15f) : 0f;

                if (spotLight)
                {
                    spotLight.enabled = intensityMult > 0.01f;
                    spotLight.intensity = spotIntensity * intensityMult;
                }
                if (fillLight)
                {
                    fillLight.enabled = intensityMult > 0.01f;
                    fillLight.intensity = fillIntensity * intensityMult;
                }

                UpdateBulbEmission(intensityMult);

                elapsed += step;
                yield return new WaitForSeconds(step);
            }

            // Restore normal light state
            if (spotLight)
            {
                spotLight.enabled = true;
                spotLight.intensity = spotIntensity;
            }
            if (fillLight)
            {
                fillLight.enabled = true;
                fillLight.intensity = fillIntensity;
            }
            UpdateBulbEmission(1.0f);
            _flickerRoutine = null;
        }

        private void UpdateBulbEmission(float level)
        {
            if (!bulbMaterial && bulbFilamentRenderer)
            {
                bulbMaterial = bulbFilamentRenderer.material;
            }

            if (bulbMaterial)
            {
                Color emissionColor = new Color(1.0f, 0.88f, 0.65f) * (level * 3.8f);
                bulbMaterial.SetColor("_EmissionColor", emissionColor);
                if (level > 0.01f)
                {
                    bulbMaterial.EnableKeyword("_EMISSION");
                }
                else
                {
                    bulbMaterial.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}

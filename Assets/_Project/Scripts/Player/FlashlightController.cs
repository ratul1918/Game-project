using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    /// <summary>
    /// Heavy 1940s-style first-person handheld vintage flashlight (torch) system.
    /// Handles mechanical toggle, camera sway inertia, breathing/walking bobbing,
    /// dual primary spot + ambient fill lights with dirty glass cookie, and horror flicker routines.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlashlightController : MonoBehaviour
    {
        [Header("Lighting Components")]
        [SerializeField] private Light spotLight;
        [SerializeField] private Light fillLight;
        [SerializeField] private Texture2D lensCookie;
        [SerializeField] private Color lightColor = new Color(1.0f, 0.957f, 0.839f); // #FFF4D6 Incandescent White-Yellow
        [SerializeField] private float spotIntensity = 24f; // ~2200 lumens equivalent
        [SerializeField] private float fillIntensity = 0.85f; // ~80 lumens soft ambient fill
        [SerializeField] private float spotRange = 22f;
        [SerializeField] private float fillRange = 1.25f;
        [SerializeField] private float spotInnerAngle = 18f;
        [SerializeField] private float spotOuterAngle = 38f;

        [Header("Flashlight State")]
        [SerializeField] private bool startsOn = true;
        [SerializeField] private bool isOn = true;

        [Header("Audio FX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip switchOnClip;
        [SerializeField] private AudioClip switchOffClip;
        [SerializeField] private AudioClip flickerSparksClip;

        [Header("Sway & Inertia (Lag)")]
        [SerializeField] private float swayAmount = 2.4f;
        [SerializeField] private float maxSwayAngle = 7.0f;
        [SerializeField] private float swaySmoothSpeed = 8.5f;

        [Header("Bobbing")]
        [SerializeField] private float idleBobSpeed = 1.4f;
        [SerializeField] private float idleBobAmount = 0.0035f;
        [SerializeField] private float walkBobSpeed = 7.5f;
        [SerializeField] private float walkBobAmount = 0.012f;

        [Header("Model References")]
        [SerializeField] private Transform flashlightModelRoot;
        [SerializeField] private Renderer bulbFilamentRenderer;
        [SerializeField] private Material bulbMaterial;

        [Header("Input Action Optional")]
        [SerializeField] private InputActionReference toggleAction;

        private FirstPersonController _fpc;
        private Transform _parentCamera;
        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private Quaternion _swayRot;
        private Quaternion _lastCamRot;
        private Coroutine _flickerRoutine;
        private float _bobTimer;

        public bool IsOn => isOn;

        private void Awake()
        {
            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;
            _swayRot = Quaternion.identity;

            _parentCamera = transform.parent;
            if (_parentCamera) _lastCamRot = _parentCamera.rotation;

            _fpc = GetComponentInParent<FirstPersonController>();

            EnsureComponents();
        }

        private void Start()
        {
            SetFlashlightOn(startsOn, playAudio: false);
        }

        private void OnEnable()
        {
            if (toggleAction != null && toggleAction.action != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += OnTogglePerformed;
            }
        }

        private void OnDisable()
        {
            if (toggleAction != null && toggleAction.action != null)
            {
                toggleAction.action.performed -= OnTogglePerformed;
                toggleAction.action.Disable();
            }

            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext ctx)
        {
            Toggle();
        }

        private void Update()
        {
            if (GetComponentInParent<FPVFlashlightHandsController>()) return;

            // Keyboard 'F' key and Gamepad D-pad Up fallback checks via New Input System
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                Toggle();
            }
            else if (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                Toggle();
            }

            UpdateBobbing(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (GetComponentInParent<FPVFlashlightHandsController>()) return;
            UpdateSway(Time.deltaTime);
        }

        public void Toggle()
        {
            var fpv = GetComponentInParent<FPVFlashlightHandsController>();
            if (fpv)
            {
                fpv.Toggle();
                isOn = fpv.IsOn;
                return;
            }
            SetFlashlightOn(!isOn, playAudio: true);
        }

        public void SetFlashlightOn(bool state, bool playAudio = true)
        {
            isOn = state;

            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }

            if (spotLight) spotLight.enabled = isOn;
            if (fillLight) fillLight.enabled = isOn;

            UpdateBulbEmission(isOn ? 1.0f : 0.0f);

            if (playAudio && audioSource)
            {
                AudioClip clip = isOn ? switchOnClip : switchOffClip;
                if (clip) audioSource.PlayOneShot(clip, 0.95f);
            }

            GameEvents.RaiseFlashlightToggled(isOn);
        }

        public void TriggerFlicker(float duration = 1.4f, int flickerCount = 7)
        {
            var fpv = GetComponentInParent<FPVFlashlightHandsController>();
            if (fpv)
            {
                fpv.TriggerFlicker(duration, flickerCount);
                return;
            }

            if (!gameObject.activeInHierarchy || !enabled) return;

            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
            }
            _flickerRoutine = StartCoroutine(FlickerRoutine(duration, flickerCount));
        }

        private IEnumerator FlickerRoutine(float duration, int flickerCount)
        {
            if (flickerSparksClip && audioSource)
            {
                audioSource.PlayOneShot(flickerSparksClip, 0.85f);
            }

            float elapsed = 0f;
            float step = duration / Mathf.Max(2, flickerCount * 2);

            while (elapsed < duration)
            {
                // Rapid micro-drop in intensity
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

                float wait = Random.Range(step * 0.4f, step * 1.6f);
                elapsed += wait;
                yield return new WaitForSeconds(wait);
            }

            // Restore steady state based on current isOn state
            if (spotLight)
            {
                spotLight.enabled = isOn;
                spotLight.intensity = spotIntensity;
            }

            if (fillLight)
            {
                fillLight.enabled = isOn;
                fillLight.intensity = fillIntensity;
            }

            UpdateBulbEmission(isOn ? 1.0f : 0.0f);
            _flickerRoutine = null;
        }

        private void UpdateBulbEmission(float level)
        {
            if (bulbMaterial)
            {
                if (level > 0.02f)
                {
                    bulbMaterial.EnableKeyword("_EMISSION");
                    bulbMaterial.SetColor("_EmissionColor", lightColor * (level * 2.5f));
                }
                else
                {
                    bulbMaterial.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        private void UpdateSway(float dt)
        {
            if (!_parentCamera) return;

            // Compute camera angular turn velocity
            Quaternion currentCamRot = _parentCamera.rotation;
            Quaternion deltaRot = Quaternion.Inverse(_lastCamRot) * currentCamRot;
            _lastCamRot = currentCamRot;

            float yawDelta = Mathf.DeltaAngle(0f, deltaRot.eulerAngles.y);
            float pitchDelta = Mathf.DeltaAngle(0f, deltaRot.eulerAngles.x);

            // Invert delta so flashlight lags slightly behind camera snappy turns
            float targetYaw = Mathf.Clamp(-yawDelta * swayAmount, -maxSwayAngle, maxSwayAngle);
            float targetPitch = Mathf.Clamp(pitchDelta * swayAmount, -maxSwayAngle, maxSwayAngle);

            Quaternion targetSway = Quaternion.Euler(targetPitch, targetYaw, -targetYaw * 0.35f);
            _swayRot = Quaternion.Slerp(_swayRot, targetSway, swaySmoothSpeed * dt);

            // Apply orientation lag relative to default offset
            transform.localRotation = _initialLocalRot * _swayRot;
        }

        private void UpdateBobbing(float dt)
        {
            float speed = _fpc != null ? _fpc.Speed : 0f;
            bool isMoving = speed > 0.15f;

            float bobSpeed = isMoving ? walkBobSpeed * Mathf.Clamp(speed / 3.0f, 0.6f, 1.8f) : idleBobSpeed;
            float bobAmp = isMoving ? walkBobAmount : idleBobAmount;

            _bobTimer += dt * bobSpeed;

            float bobX = Mathf.Sin(_bobTimer * 0.5f) * (bobAmp * 0.6f);
            float bobY = Mathf.Cos(_bobTimer) * bobAmp;

            transform.localPosition = _initialLocalPos + new Vector3(bobX, bobY, 0f);
        }

        public void SetupLightsAndAudio()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            if (audioSource)
            {
                audioSource.spatialBlend = 0f; // 2D player audio
                audioSource.playOnAwake = false;
                audioSource.priority = 48;
            }

            if (!switchOnClip) switchOnClip = Resources.Load<AudioClip>("Flashlight_Switch_On") ?? LoadAudio("Flashlight_Switch_On.wav");
            if (!switchOffClip) switchOffClip = Resources.Load<AudioClip>("Flashlight_Switch_Off") ?? LoadAudio("Flashlight_Switch_Off.wav");
            if (!flickerSparksClip) flickerSparksClip = Resources.Load<AudioClip>("Flashlight_Flicker_Sparks") ?? LoadAudio("Flashlight_Flicker_Sparks.wav");

            if (!lensCookie)
            {
#if UNITY_EDITOR
                lensCookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/Lighting/flashlight_cookie_grungy.png");
#endif
            }

            // Configure Spot Light
            if (!spotLight)
            {
                var existingSpot = transform.Find("Flashlight_Primary_Spot");
                var spotGo = existingSpot ? existingSpot.gameObject : new GameObject("Flashlight_Primary_Spot");
                spotGo.transform.SetParent(transform, false);
                spotGo.transform.localPosition = new Vector3(0, 0, 0.15f);
                spotGo.transform.localRotation = Quaternion.identity;

                spotLight = spotGo.GetComponent<Light>();
                if (!spotLight) spotLight = spotGo.AddComponent<Light>();
            }

            if (spotLight)
            {
                spotLight.type = LightType.Spot;
                spotLight.range = spotRange;
                spotLight.innerSpotAngle = spotInnerAngle;
                spotLight.spotAngle = spotOuterAngle;
                spotLight.color = lightColor;
                spotLight.intensity = spotIntensity;
                spotLight.shadows = LightShadows.Soft;
                if (lensCookie) spotLight.cookie = lensCookie;
            }

            // Configure Fill Light
            if (!fillLight)
            {
                var existingFill = transform.Find("Flashlight_Ambient_Fill");
                var fillGo = existingFill ? existingFill.gameObject : new GameObject("Flashlight_Ambient_Fill");
                fillGo.transform.SetParent(transform, false);
                fillGo.transform.localPosition = new Vector3(0, 0, 0.15f);

                fillLight = fillGo.GetComponent<Light>();
                if (!fillLight) fillLight = fillGo.AddComponent<Light>();
            }

            if (fillLight)
            {
                fillLight.type = LightType.Point;
                fillLight.range = fillRange;
                fillLight.color = lightColor;
                fillLight.intensity = fillIntensity;
                fillLight.shadows = LightShadows.None;
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

        public void AssignModelReferences(Transform modelRoot, Renderer bulbRend, Material bulbMat)
        {
            flashlightModelRoot = modelRoot;
            bulbFilamentRenderer = bulbRend;
            bulbMaterial = bulbMat;
            UpdateBulbEmission(isOn ? 1.0f : 0.0f);
        }
    }
}

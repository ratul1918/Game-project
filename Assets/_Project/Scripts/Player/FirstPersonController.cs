using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NoiseEmitter))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const float MovementInputThreshold = 0.01f;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string lookActionName = "Look";
        [SerializeField] private string sprintActionName = "Sprint";
        [SerializeField] private string crouchActionName = "Crouch";

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private NoiseEmitter noiseEmitter;

        [SerializeField] private PlayerMovementSettings settings;
        [SerializeField] private string jumpActionName = "Jump";
        [SerializeField] private string leanLeftActionName = "LeanLeft";
        [SerializeField] private string leanRightActionName = "LeanRight";
        [SerializeField] private string debugActionName = "MovementDebug";
        private InputAction jumpAction, leanLeftAction, leanRightAction, debugAction;
        private PlayerStamina stamina;
        private HeadBob bob;
        private CameraEffects effects;
        private FootstepSystem footsteps;
        private FootstepSurface surface;
        private Lean lean;
        private FallDamageSafety safety;
        private PlayerDebugPanel debugPanel;
        private Vector3 horizontalVelocity;
        private float lastGroundedTime = float.NegativeInfinity;
        private float lastJumpTime = float.NegativeInfinity;
        private bool lookLocked;
        private float peekYawLimit, peekPitchLimit, peekYaw;
        public void SetPeekLimits(float yaw, float pitchLimit)
        {
            peekYawLimit=yaw;peekPitchLimit=pitchLimit;peekYaw=0f;pitch=0f;
        }
        private bool hidden;
        private bool crouching;
        private bool sprinting;
        private bool movementEnabled = true;
        public float Speed { get; private set; }
        public bool Grounded { get; private set; }
        public PlayerStamina Stamina => stamina;
        public FootstepSurface Surface => surface;
        public PlayerMovementSettings Settings => settings;
        private CharacterController characterController;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private float verticalVelocity;
        private float pitch;
        public bool MovementEnabled
        {
            get => movementEnabled;
            set { movementEnabled = value; if (!value) { horizontalVelocity = Vector3.zero; lastJumpTime = float.NegativeInfinity; } }
        }
        public void SetMovementLocked(bool locked) => MovementEnabled = !locked;
        public void SetLookLocked(bool locked) => lookLocked = locked;
        private void Hidden(PlayerHiddenEvent e) { hidden = e.IsHidden; if (hidden) horizontalVelocity = Vector3.zero; }
        private T Ensure<T>() where T : Component => GetComponent<T>() ?? gameObject.AddComponent<T>();

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            noiseEmitter ??= GetComponent<NoiseEmitter>();

            if (!settings) settings = Resources.Load<PlayerMovementSettings>("PlayerMovementSettings");
            if (!settings) settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            stamina = Ensure<PlayerStamina>();
            stamina.Initialize(settings);
            surface = Ensure<FootstepSurface>();
            footsteps = Ensure<FootstepSystem>();
            footsteps.Initialize();
            bob = Ensure<HeadBob>();
            effects = Ensure<CameraEffects>();
            lean = Ensure<Lean>();
            safety = Ensure<FallDamageSafety>();
            safety.Initialize();
            debugPanel = Ensure<PlayerDebugPanel>();
            debugPanel.Initialize(this);
            Ensure<StaminaBar>();
            if (cameraTransform && cameraTransform.TryGetComponent<Camera>(out var viewCamera))
                viewCamera.nearClipPlane = settings.cameraNearClip;
            characterController.slopeLimit = settings.slopeLimit;
            characterController.skinWidth = settings.skinWidth;
            ResolveActions();
        }

        private void OnEnable()
        {
            GameEvents.PlayerHidden += Hidden;
            EnableActions();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHidden -= Hidden;
            DisableActions();
        }

        private void Update()
        {
            if (moveAction == null || lookAction == null || cameraTransform == null)
            {
                return;
            }

            if (debugAction != null && debugAction.WasPressedThisFrame()) debugPanel.Toggle();
            UpdateLook();
            UpdateMovement();
        }

        private void ResolveActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("FirstPersonController requires an InputActionAsset.", this);
                return;
            }

            InputActionMap actionMap = inputActions.FindActionMap(actionMapName, true);
            moveAction = actionMap.FindAction(moveActionName, true);
            lookAction = actionMap.FindAction(lookActionName, true);
            sprintAction = actionMap.FindAction(sprintActionName, true);
            crouchAction = actionMap.FindAction(crouchActionName, true);
            jumpAction = actionMap.FindAction(jumpActionName, false);
            leanLeftAction = actionMap.FindAction(leanLeftActionName, false);
            leanRightAction = actionMap.FindAction(leanRightActionName, false);
            debugAction = actionMap.FindAction(debugActionName, false);
        }

        private void EnableActions()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            sprintAction?.Enable();
            crouchAction?.Enable();
            jumpAction?.Enable();
            leanLeftAction?.Enable();
            leanRightAction?.Enable();
            debugAction?.Enable();
        }

        private void DisableActions()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
            jumpAction?.Disable();
            leanLeftAction?.Disable();
            leanRightAction?.Disable();
            debugAction?.Disable();
        }

        private void UpdateLook()
        {
            Vector2 lookInput = lookLocked ? Vector2.zero : lookAction.ReadValue<Vector2>() * settings.lookSensitivity;
            pitch = Mathf.Clamp(pitch - lookInput.y, settings.minimumPitch, settings.maximumPitch);
            if (peekPitchLimit > 0f) pitch = Mathf.Clamp(pitch, -peekPitchLimit, peekPitchLimit);
            if (peekYawLimit > 0f)
            {
                float nextYaw = Mathf.Clamp(peekYaw + lookInput.x, -peekYawLimit, peekYawLimit);
                lookInput.x = nextYaw - peekYaw; peekYaw = nextYaw;
            }
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.Rotate(Vector3.up, lookInput.x, Space.Self);
        }

        private void UpdateMovement()
        {
            float dt = Time.deltaTime;
            if (hidden)
            {
                horizontalVelocity=Vector3.zero;verticalVelocity=0f;Speed=0f;Grounded=false;
                lastJumpTime=float.NegativeInfinity;
                stamina.Tick(false,dt);bob.Tick(false,0f,settings,dt);
                effects.Tick(cameraTransform,pitch,characterController.height,0f,false,
                    stamina.Normalized,0f,settings,dt);
                return;
            }
            bool wasGrounded = Grounded;
            float incomingFallSpeed = Mathf.Max(0f, -verticalVelocity);
            bool canMove = MovementEnabled && !hidden;
            Vector2 input = canMove ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
            if (canMove && jumpAction != null && jumpAction.WasPressedThisFrame()) lastJumpTime = Time.time;
            if (!canMove) lastJumpTime = float.NegativeInfinity;
            bool requestedCrouch = canMove && crouchAction != null && crouchAction.IsPressed();
            UpdateCrouch(requestedCrouch);

            bool walkableProbe = ProbeGround(out RaycastHit groundHit);
            // At a riser the sphere can hit its vertical face before the floor. Preserve the
            // controller's below-contact so that stepOffset stays enabled for stair climbing.
            bool groundBefore = (characterController.isGrounded || walkableProbe) && verticalVelocity <= 0f;
            surface.Detect(groundHit, settings);
            if (groundBefore) lastGroundedTime = Time.time;
            sprinting = canMove && !crouching && groundBefore && input.sqrMagnitude > MovementInputThreshold
                && sprintAction != null && sprintAction.IsPressed() && stamina.CanSprint;
            float speed = crouching ? settings.crouchSpeed : sprinting ? settings.sprintSpeed : settings.walkSpeed;
            Vector3 target = (transform.right * input.x + transform.forward * input.y) * speed;
            float rate = input.sqrMagnitude > MovementInputThreshold ? settings.acceleration : settings.deceleration;
            horizontalVelocity = canMove ? Vector3.MoveTowards(horizontalVelocity, target,
                rate * (groundBefore ? 1f : settings.airControl) * dt) : Vector3.zero;
            if (groundBefore && verticalVelocity < 0f) verticalVelocity = settings.groundedVelocity;
            if (canMove && !crouching && Time.time - lastGroundedTime <= settings.coyoteTime
                && Time.time - lastJumpTime <= settings.jumpBuffer && stamina.TryJump())
            {
                verticalVelocity = Mathf.Sqrt(2f * Physics.gravity.magnitude * settings.gravityMultiplier * settings.jumpHeight);
                lastJumpTime = lastGroundedTime = float.NegativeInfinity;
                footsteps.Takeoff(settings);
                groundBefore = false;
            }

            verticalVelocity = Mathf.Max(verticalVelocity - Physics.gravity.magnitude * settings.gravityMultiplier * dt,
                -settings.terminalVelocity);
            float impactSpeed = Mathf.Max(incomingFallSpeed, -verticalVelocity);
            characterController.stepOffset = groundBefore ? Mathf.Min(settings.stepOffset, characterController.height) : 0f;
            Vector3 previousPosition = transform.position;
            CollisionFlags flags = characterController.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
            Grounded = verticalVelocity <= 0f && ((flags & CollisionFlags.Below) != 0 || ProbeGround(out groundHit));
            if (Grounded) surface.Detect(groundHit, settings);
            if (Grounded && !wasGrounded && impactSpeed >= settings.minimumLandingSpeed)
            {
                footsteps.Land(impactSpeed, settings);
                effects.Land(impactSpeed, settings);
            }
            Vector3 actual = transform.position - previousPosition;
            actual.y = 0f;
            Speed = dt > 0f ? actual.magnitude / dt : 0f;
            stamina.Tick(sprinting && Speed > settings.movementThreshold, dt);
            bool moving = Grounded && canMove && Speed > settings.movementThreshold;
            if (bob.Tick(moving, Speed, settings, dt)) footsteps.Step(crouching, sprinting, settings);

            Vector3 neutralView = transform.TransformPoint(new Vector3(0f, cameraTransform.localPosition.y, 0f));
            float leanInput = lookLocked || hidden ? 0f :
                (leanRightAction != null && leanRightAction.IsPressed() ? 1f : 0f)
                - (leanLeftAction != null && leanLeftAction.IsPressed() ? 1f : 0f);
            lean.Tick(leanInput, transform, neutralView, settings, dt);
            effects.Tick(cameraTransform, pitch, characterController.height, bob.Offset, sprinting,
                stamina.Normalized, lean.Amount, settings, dt);
            if (safety.Tick(characterController, Grounded, settings, dt))
            {
                verticalVelocity = 0f; horizontalVelocity = Vector3.zero; bob.ResetPhase();
                lastJumpTime = lastGroundedTime = float.NegativeInfinity;
            }
        }

        private bool ProbeGround(out RaycastHit hit)
        {
            float radius = Mathf.Max(settings.groundProbeInset, characterController.radius - settings.groundProbeInset);
            Vector3 origin = transform.position + Vector3.up * (characterController.radius + settings.groundProbeInset);
            if (!Physics.SphereCast(origin, radius, Vector3.down, out hit,
                settings.groundProbeDistance + settings.groundProbeInset * 2f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return false;
            return Vector3.Angle(hit.normal, Vector3.up) <= settings.slopeLimit;
        }

        private void UpdateCrouch(bool requested)
        {
            bool blocked = false;
            if (!requested && characterController.height < settings.standingHeight)
            {
                Vector3 lower = transform.position + Vector3.up *
                    (characterController.height - characterController.radius + settings.ceilingClearance);
                Vector3 upper = transform.position + Vector3.up *
                    (settings.standingHeight - characterController.radius + settings.ceilingClearance);
                // Test only the additional headroom, excluding our own CharacterController.
                foreach (Collider collider in Physics.OverlapCapsule(lower, upper,
                    characterController.radius - settings.skinWidth, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    if (collider != characterController && !collider.transform.IsChildOf(transform)) { blocked = true; break; }
            }
            crouching = requested || blocked;
            float target = crouching ? settings.crouchingHeight : settings.standingHeight;
            characterController.height = Mathf.MoveTowards(characterController.height, target,
                settings.crouchTransitionSpeed * Time.deltaTime);
            characterController.center = Vector3.up * (characterController.height * .5f);
            // Don't allow a jump until the capsule has completely stood up.
            if (characterController.height < settings.standingHeight - settings.ceilingClearance) crouching = true;
        }
    }
}

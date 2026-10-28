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

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float crouchSpeed = 2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVelocity = -2f;

        [Header("Crouch")]
        [SerializeField, Min(0.1f)] private float standingHeight = 1.8f;
        [SerializeField, Min(0.1f)] private float crouchingHeight = 1.1f;
        [SerializeField, Min(0f)] private float crouchTransitionSpeed = 10f;

        [Header("Look")]
        [SerializeField, Min(0f)] private float lookSensitivity = 0.08f;
        [SerializeField] private float minimumPitch = -85f;
        [SerializeField] private float maximumPitch = 85f;

        [Header("Head Bob")]
        [SerializeField, Min(0f)] private float headBobAmplitude = 0.045f;
        [SerializeField, Min(0f)] private float headBobFrequency = 10f;
        [SerializeField, Min(0f)] private float headBobReturnSpeed = 12f;

        [Header("Footsteps")]
        [SerializeField, Min(0.05f)] private float walkStepInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float sprintStepInterval = 0.34f;
        [SerializeField, Min(0.05f)] private float crouchStepInterval = 0.7f;
        [SerializeField, Min(0f)] private float crouchLoudness = 2f;
        [SerializeField, Min(0f)] private float walkLoudness = 6f;
        [SerializeField, Min(0f)] private float sprintLoudness = 12f;

        private CharacterController characterController;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private Vector3 cameraRestLocalPosition;
        private float verticalVelocity;
        private float pitch;
        private float headBobTime;
        private float stepTimer;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            noiseEmitter ??= GetComponent<NoiseEmitter>();

            if (cameraTransform != null)
            {
                cameraRestLocalPosition = cameraTransform.localPosition;
            }

            ResolveActions();
        }

        private void OnEnable()
        {
            EnableActions();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void Update()
        {
            if (moveAction == null || lookAction == null || cameraTransform == null)
            {
                return;
            }

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
        }

        private void EnableActions()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            sprintAction?.Enable();
            crouchAction?.Enable();
        }

        private void DisableActions()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
        }

        private void UpdateLook()
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>() * lookSensitivity;
            pitch = Mathf.Clamp(pitch - lookInput.y, minimumPitch, maximumPitch);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.Rotate(Vector3.up, lookInput.x, Space.Self);
        }

        private void UpdateMovement()
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            bool isCrouching = crouchAction != null && crouchAction.IsPressed();
            bool isSprinting = !isCrouching && sprintAction != null && sprintAction.IsPressed();

            UpdateCrouch(isCrouching);

            float speed = isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed;
            Vector3 horizontalMovement =
                (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedVelocity;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement + Vector3.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);

            bool isMoving = moveInput.sqrMagnitude > MovementInputThreshold;
            UpdateHeadBob(isMoving && characterController.isGrounded, speed);
            UpdateFootsteps(isMoving && characterController.isGrounded, isCrouching, isSprinting);
        }

        private void UpdateCrouch(bool isCrouching)
        {
            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            characterController.height = Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                crouchTransitionSpeed * Time.deltaTime);
            characterController.center = Vector3.up * (characterController.height * 0.5f);
        }

        private void UpdateHeadBob(bool isMoving, float movementSpeed)
        {
            Vector3 targetPosition = cameraRestLocalPosition;
            if (isMoving)
            {
                float speedRatio = movementSpeed / Mathf.Max(walkSpeed, MovementInputThreshold);
                headBobTime += Time.deltaTime * headBobFrequency * speedRatio;
                targetPosition.y += Mathf.Sin(headBobTime) * headBobAmplitude;
            }

            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                targetPosition,
                headBobReturnSpeed * Time.deltaTime);
        }

        private void UpdateFootsteps(bool isMoving, bool isCrouching, bool isSprinting)
        {
            if (!isMoving)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer > 0f)
            {
                return;
            }

            float loudness = isCrouching ? crouchLoudness : isSprinting ? sprintLoudness : walkLoudness;
            float interval = isCrouching
                ? crouchStepInterval
                : isSprinting
                    ? sprintStepInterval
                    : walkStepInterval;

            noiseEmitter.Emit(loudness);
            stepTimer = interval;
        }
    }
}

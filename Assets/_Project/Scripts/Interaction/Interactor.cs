using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    public sealed class Interactor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Raycast")]
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = ~0;

        [Header("UI")]
        [SerializeField] private InteractionPrompt interactionPrompt;

        private readonly RaycastHit[] raycastHits = new RaycastHit[32];
        private InputAction interactAction;
        private IInteractable focusedInteractable;
        private bool isInteracting;

        private void Awake()
        {
            ResolveAction();
        }

        private void OnEnable()
        {
            interactAction?.Enable();
        }

        private void OnDisable()
        {
            StopInteraction();
            interactAction?.Disable();
            interactionPrompt?.Hide();
        }

        private void Update()
        {
            IInteractable nextInteractable = FindFocusedInteractable();
            if (!ReferenceEquals(nextInteractable, focusedInteractable))
            {
                StopInteraction();
                focusedInteractable = nextInteractable;
                UpdatePrompt();
            }

            if (focusedInteractable == null || interactAction == null)
            {
                return;
            }

            if (interactAction.WasPressedThisFrame() ||
                (focusedInteractable.RequiresHold && interactAction.IsPressed()))
            {
                isInteracting = true;
                focusedInteractable.Interact();
            }
            else if (isInteracting && !interactAction.IsPressed())
            {
                StopInteraction();
            }
        }

        private void ResolveAction()
        {
            if (inputActions == null)
            {
                Debug.LogError("Interactor requires an InputActionAsset.", this);
                return;
            }

            InputActionMap actionMap = inputActions.FindActionMap(actionMapName, true);
            interactAction = actionMap.FindAction(interactActionName, true);
        }

        private IInteractable FindFocusedInteractable()
        {
            if (viewCamera == null)
            {
                return null;
            }

            Ray ray = new(viewCamera.transform.position, viewCamera.transform.forward);
            int count = Physics.RaycastNonAlloc(ray, raycastHits, interactionDistance,
                interactionLayers, QueryTriggerInteraction.Ignore);
            Collider closest = null;
            float distance = float.PositiveInfinity;
            for (int index = 0; index < count; index++)
            {
                var hit = raycastHits[index];
                if (hit.collider.transform.IsChildOf(transform) || hit.distance >= distance) continue;
                closest = hit.collider;
                distance = hit.distance;
            }
            if (closest == null) return null;

            MonoBehaviour[] behaviours = closest.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (!behaviour.isActiveAndEnabled)
                {
                    continue;
                }
                if (behaviour is IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private void UpdatePrompt()
        {
            if (focusedInteractable == null)
            {
                interactionPrompt?.Hide();
            }
            else
            {
                interactionPrompt?.Show(focusedInteractable.Prompt);
            }
        }

        private void StopInteraction()
        {
            if (isInteracting && focusedInteractable != null)
            {
                focusedInteractable.StopInteract();
            }

            isInteracting = false;
        }
    }
}

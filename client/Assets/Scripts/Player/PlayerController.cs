using UnityEngine;
using UnityEngine.InputSystem;
using EmberBlade.Config;
using EmberBlade.Missions;
using EmberBlade.Combat;

namespace EmberBlade.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private ThirdPersonCamera followCamera;

        private CharacterController characterController;
        private Health health;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (health != null && health.IsDead)
            {
                return;
            }
            if (DialogueUI.IsAnyDialogueOpen)
            {
                return;
            }

            Vector2 input = ReadMoveInput();
            Vector3 forward = followCamera != null ? followCamera.PlanarForward : Vector3.forward;
            Vector3 right = followCamera != null ? followCamera.PlanarRight : Vector3.right;
            Vector3 movement = (forward * input.y + right * input.x);
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            if (movement.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    (config != null ? config.playerRotationSpeed : 12f) * Time.deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            characterController.Move((movement * (config != null ? config.playerMoveSpeed : 5f)
                + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private static Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null)
            {
                return Vector2.zero;
            }

            return new Vector2(
                (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f));
        }
    }
}

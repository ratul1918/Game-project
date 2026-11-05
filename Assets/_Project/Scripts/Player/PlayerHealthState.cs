using System;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class PlayerHealthState : MonoBehaviour
    {
        [Header("Survival Health & Injury")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float bleedRate = 1.2f;
        [SerializeField] private float lowHealthThreshold = 30f;
        [SerializeField] private float limpSpeedMultiplier = 0.65f;

        [Header("Respawn & Hiding")]
        [SerializeField] private Transform checkpoint;

        private FirstPersonController controller;
        private CharacterController capsule;
        private Interactor interactor;
        private Vector3 exitPosition;
        private Quaternion exitRotation;
        private bool isBleeding;

        public bool IsHidden { get; private set; }
        public bool IsCaught { get; set; }
        public Vector3 CheckpointPosition { get; private set; }
        public HidingSpot CurrentSpot { get; private set; }

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsBleeding => isBleeding;
        public float NormalizedHealth => Mathf.Clamp01(currentHealth / maxHealth);

        public event Action<float, float> HealthChanged;
        public event Action<bool> BleedingChanged;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            capsule = GetComponent<CharacterController>();
            interactor = GetComponent<Interactor>();
            CheckpointPosition = checkpoint ? checkpoint.position : transform.position;
            currentHealth = maxHealth;
        }

        private void OnEnable()
        {
            GameEvents.HidingEntered += Enter;
            GameEvents.HidingExited += Exit;
            GameEvents.GameStateChanged += State;
        }

        private void OnDisable()
        {
            GameEvents.HidingEntered -= Enter;
            GameEvents.HidingExited -= Exit;
            GameEvents.GameStateChanged -= State;
        }

        private void Update()
        {
            if (IsCaught || IsHidden) return;

            // Bleeding over time
            if (isBleeding && currentHealth > 0f)
            {
                TakeDamage(bleedRate * Time.deltaTime, false);
            }

            // Limping movement penalty
            if (controller)
            {
                controller.MoveSpeedMultiplier = (currentHealth <= lowHealthThreshold) ? limpSpeedMultiplier : 1.0f;
            }
        }

        public void TakeDamage(float damage, bool causeBleeding = false)
        {
            if (IsCaught || currentHealth <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (causeBleeding && !isBleeding)
            {
                isBleeding = true;
                BleedingChanged?.Invoke(true);
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (currentHealth <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ApplyBandage(float healAmount = 25f)
        {
            isBleeding = false;
            BleedingChanged?.Invoke(false);
            Heal(healAmount);
        }

        private void Die()
        {
            if (IsCaught) return;
            IsCaught = true;
            if (controller)
            {
                controller.SetMovementLocked(true);
                controller.SetLookLocked(true);
            }
            GameEvents.RaisePlayerDeath();
            var deathUI = FindAnyObjectByType<DeathScreenUI>();
            if (deathUI) deathUI.ShowDeath();
        }

        private void State(GameStateChangedEvent e)
        {
            if (e.Current == GameState.Storm || e.Current == GameState.Finale || e.Current == GameState.Dawn)
            {
                CheckpointPosition = transform.position;
                GameEvents.RaiseCheckpointReached(new(e.Current.ToString(), CheckpointPosition));
            }
        }

        public void SetCheckpoint(Vector3 position)
        {
            CheckpointPosition = position;
        }

        private void Enter(HidingEnteredEvent request)
        {
            var spot = request.Spot;
            if (IsHidden || IsCaught) return;
            exitPosition = transform.position;
            exitRotation = transform.rotation;
            CurrentSpot = spot;
            IsHidden = true;
            if (capsule) capsule.enabled = false;
            transform.SetPositionAndRotation(spot.Pose.position, spot.Pose.rotation);
            if (interactor) interactor.enabled = false;
            if (controller)
            {
                controller.SetMovementLocked(true);
                controller.SetPeekLimits(spot.PeekYaw, spot.PeekPitch);
            }
            GameEvents.RaisePlayerHidden(new(true, spot.Id));
        }

        private void Exit()
        {
            if (!IsHidden) return;
            transform.SetPositionAndRotation(exitPosition, exitRotation);
            if (capsule) capsule.enabled = true;
            if (controller)
            {
                controller.SetPeekLimits(0, 0);
                controller.SetMovementLocked(false);
            }
            if (interactor) interactor.enabled = true;
            IsHidden = false;
            CurrentSpot = null;
            GameEvents.RaisePlayerHidden(new(false, ""));
        }

        public void Respawn()
        {
            Exit();
            if (capsule) capsule.enabled = false;
            transform.position = CheckpointPosition;
            if (capsule) capsule.enabled = true;
            IsCaught = false;
            currentHealth = maxHealth;
            isBleeding = false;
            HealthChanged?.Invoke(currentHealth, maxHealth);
            BleedingChanged?.Invoke(false);
            if (controller)
            {
                controller.MoveSpeedMultiplier = 1.0f;
                controller.SetMovementLocked(false);
                controller.SetLookLocked(false);
            }
            GameEvents.RaisePlayerRespawned();
        }
    }
}

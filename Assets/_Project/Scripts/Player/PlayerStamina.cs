using UnityEngine;

namespace TheLightHouse
{
    public sealed class PlayerStamina : MonoBehaviour
    {
        PlayerMovementSettings settings;
        float exhaustedUntil;
        public float Current { get; private set; }
        public float Normalized => settings ? Current / settings.staminaCapacity : 1f;
        public bool CanSprint => settings && Current > 0f && Time.time >= exhaustedUntil;
        public void Initialize(PlayerMovementSettings value)
        {
            settings = value;
            Current = settings.staminaCapacity;
            Publish();
        }
        public void Tick(bool sprinting, float deltaTime)
        {
            float previous = Current;
            Current = Mathf.Clamp(Current + (sprinting ? -settings.sprintDrain : settings.staminaRegeneration) * deltaTime,
                0f, settings.staminaCapacity);
            if (Current <= 0f && previous > 0f) exhaustedUntil = Time.time + settings.exhaustionLockout;
            if (!Mathf.Approximately(previous, Current)) Publish();
        }
        public bool TryJump()
        {
            if (!CanSprint || Current < settings.jumpCost) return false;
            Current -= settings.jumpCost;
            if (Current <= 0f) exhaustedUntil = Time.time + settings.exhaustionLockout;
            Publish();
            return true;
        }
        void Publish() => GameEvents.RaiseStaminaChanged(new(Current, settings.staminaCapacity, Time.time < exhaustedUntil));
    }
}

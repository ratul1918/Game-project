using System;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class WeaponDurability : MonoBehaviour
    {
        [Header("Durability Settings")]
        [SerializeField] private int maxDurability = 50;
        [SerializeField] private int currentDurability = 50;
        [SerializeField] private float dullDamageMultiplier = 0.6f;

        public int MaxDurability => maxDurability;
        public int CurrentDurability => currentDurability;
        public bool IsDull => currentDurability <= 0;
        public float DamageMultiplier => IsDull ? dullDamageMultiplier : 1.0f;
        public float NormalizedDurability => Mathf.Clamp01((float)currentDurability / maxDurability);

        public event Action<int, int> DurabilityChanged;

        public void Consume(int amount = 1)
        {
            if (currentDurability <= 0) return;
            currentDurability = Mathf.Max(0, currentDurability - amount);
            DurabilityChanged?.Invoke(currentDurability, maxDurability);
        }

        public void Repair(int amount = 50)
        {
            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
            DurabilityChanged?.Invoke(currentDurability, maxDurability);
        }

        public void RestoreFull()
        {
            currentDurability = maxDurability;
            DurabilityChanged?.Invoke(currentDurability, maxDurability);
        }
    }
}

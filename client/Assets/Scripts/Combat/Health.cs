using System;
using UnityEngine;
using EmberBlade.Config;

namespace EmberBlade.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0;
        public event Action<int, int> OnDamaged;
        public event Action OnDied;

        public void Initialize(int value)
        {
            maxHealth = value;
            CurrentHealth = value;
        }

        private void Awake()
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = maxHealth;
            }
        }

        public void Damage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamaged?.Invoke(CurrentHealth, maxHealth);
            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnDamaged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

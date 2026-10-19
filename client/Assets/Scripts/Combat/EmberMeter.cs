using System;
using UnityEngine;
using EmberBlade.Config;

namespace EmberBlade.Combat
{
    public class EmberMeter : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        public int Value { get; private set; }
        public int MaxValue => config != null ? config.emberMax : 100;
        public event Action<int, int> Changed;

        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
            Value = 0;
            Changed?.Invoke(Value, MaxValue);
        }

        public void Add(int amount)
        {
            Value = Mathf.Clamp(Value + amount, 0, MaxValue);
            Changed?.Invoke(Value, MaxValue);
        }

        public bool TrySpendEmpowered()
        {
            if (Value < MaxValue)
            {
                return false;
            }

            Value = 0;
            Changed?.Invoke(Value, MaxValue);
            return true;
        }
    }
}

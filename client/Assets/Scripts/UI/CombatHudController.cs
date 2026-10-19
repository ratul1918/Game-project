using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EmberBlade.Combat;
using EmberBlade.AI;

namespace EmberBlade.UI
{
    public class CombatHudController : MonoBehaviour
    {
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider emberBar;
        [SerializeField] private TMP_Text emberText;
        [SerializeField] private TMP_Text enemiesText;
        [SerializeField] private Health playerHealth;
        [SerializeField] private EmberMeter emberMeter;
        [SerializeField] private EnemyWaveManager enemyWaveManager;

        public void Initialize(
            Health health,
            EmberMeter ember,
            EnemyWaveManager enemies)
        {
            playerHealth = health;
            emberMeter = ember;
            enemyWaveManager = enemies;
            playerHealth.OnDamaged += UpdateHealth;
            emberMeter.Changed += UpdateEmber;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            UpdateEmber(emberMeter.Value, emberMeter.MaxValue);
        }

        private void Update()
        {
            if (enemyWaveManager != null && enemiesText != null)
            {
                enemiesText.text = $"Enemies remaining: {enemyWaveManager.EnemiesRemaining}";
            }
        }

        private void UpdateHealth(int current, int maximum)
        {
            if (healthBar != null)
            {
                healthBar.value = maximum > 0 ? (float)current / maximum : 0f;
            }
        }

        private void UpdateEmber(int current, int maximum)
        {
            if (emberBar != null)
            {
                emberBar.value = maximum > 0 ? (float)current / maximum : 0f;
                Image fillImage = emberBar.fillRect != null
                    ? emberBar.fillRect.GetComponent<Image>()
                    : null;
                if (fillImage != null)
                {
                    fillImage.color = current >= maximum
                        ? new Color(1f, 0.35f, 0.05f)
                        : new Color(0.95f, 0.2f, 0.05f);
                }
            }
            if (emberText != null)
            {
                emberText.text = $"Ember: {current}/{maximum}";
            }
        }
    }
}

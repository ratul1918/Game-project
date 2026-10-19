using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using EmberBlade.Config;
using EmberBlade.AI;

namespace EmberBlade.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Health health;
        [SerializeField] private EmberMeter emberMeter;

        private float nextAttackTime;
        private float nextDodgeTime;
        private float nextParryTime;
        private bool invulnerable;
        private bool parryActive;

        public bool IsInvulnerable => invulnerable;

        public void Initialize(GameConfig gameConfig, Health playerHealth, EmberMeter playerEmber)
        {
            config = gameConfig;
            health = playerHealth;
            emberMeter = playerEmber;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                TryDodge();
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TryLightAttack();
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    TryParry();
                }
            }
        }

        public bool ReceiveEnemyAttack(int damage)
        {
            if (invulnerable)
            {
                return true;
            }

            if (parryActive)
            {
                parryActive = false;
                emberMeter?.Add(config.parryEmberGain);
                return true;
            }

            health?.Damage(damage);
            return false;
        }

        private void TryLightAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + config.lightAttackCooldown;
            int damage = config.lightAttackDamage;
            if (emberMeter != null && emberMeter.TrySpendEmpowered())
            {
                damage = Mathf.RoundToInt(damage * config.emberEmpoweredDamageMultiplier);
            }

            Vector3 center = transform.position + transform.forward * config.lightAttackRange + Vector3.up;
            Collider[] hits = Physics.OverlapSphere(center, config.lightAttackRadius);
            foreach (Collider hit in hits)
            {
                EnemyController enemy = hit.GetComponentInParent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        private void TryDodge()
        {
            if (Time.time < nextDodgeTime || !isActiveAndEnabled)
            {
                return;
            }

            nextDodgeTime = Time.time + config.dodgeCooldown;
            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            invulnerable = true;
            float elapsed = 0f;
            CharacterController controller = GetComponent<CharacterController>();
            while (elapsed < config.dodgeDuration)
            {
                elapsed += Time.deltaTime;
                Vector3 movement = transform.forward * (config.dodgeDistance / config.dodgeDuration) * Time.deltaTime;
                if (controller != null)
                {
                    controller.Move(movement);
                }
                else
                {
                    transform.position += movement;
                }
                yield return null;
            }
            invulnerable = false;
        }

        private void TryParry()
        {
            if (Time.time < nextParryTime)
            {
                return;
            }

            nextParryTime = Time.time + config.parryCooldown;
            StartCoroutine(ParryRoutine());
        }

        private IEnumerator ParryRoutine()
        {
            parryActive = true;
            yield return new WaitForSeconds(config.parryWindow);
            parryActive = false;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;
using EmberBlade.Config;
using EmberBlade.Combat;

namespace EmberBlade.AI
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private bool isSkirmisher;
        [SerializeField] private Health health;
        [SerializeField] private Renderer bodyRenderer;

        private NavMeshAgent agent;
        private Transform player;
        private PlayerCombat playerCombat;
        private EnemyWaveManager waveManager;
        private EnemyState state;
        private float nextAttackTime;
        private bool windingUp;
        private Color baseColor;

        public EnemyState State => state;

        public void Initialize(
            GameConfig gameConfig,
            bool skirmisher,
            Transform playerTarget,
            PlayerCombat combat,
            EnemyWaveManager manager)
        {
            config = gameConfig;
            isSkirmisher = skirmisher;
            player = playerTarget;
            playerCombat = combat;
            waveManager = manager;
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            bodyRenderer ??= GetComponentInChildren<Renderer>();
            baseColor = bodyRenderer != null ? bodyRenderer.material.color : Color.white;
            health.Initialize(isSkirmisher ? config.skirmisherHealth : config.scoutHealth);
            health.OnDied += Die;
            state = EnemyState.Idle;
            agent.speed = isSkirmisher ? config.skirmisherSpeed : config.scoutSpeed;
            agent.stoppingDistance = isSkirmisher ? config.skirmisherAttackRange : config.scoutAttackRange;
        }

        private void Update()
        {
            if (state == EnemyState.Dead || player == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > config.enemyChaseRange)
            {
                state = EnemyState.Idle;
                agent.isStopped = true;
                return;
            }

            if (distance > agent.stoppingDistance)
            {
                state = EnemyState.Chase;
                agent.isStopped = false;
                agent.SetDestination(player.position);
                return;
            }

            state = EnemyState.Attack;
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            TryAttack();
        }

        public void TakeDamage(int amount)
        {
            health.Damage(amount);
        }

        private void TryAttack()
        {
            if (windingUp || Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + (isSkirmisher
                ? config.skirmisherAttackCooldown
                : config.scoutAttackCooldown);
            if (isSkirmisher)
            {
                StartCoroutine(WindupAndAttack());
            }
            else
            {
                playerCombat.ReceiveEnemyAttack(config.scoutDamage);
            }
        }

        private System.Collections.IEnumerator WindupAndAttack()
        {
            windingUp = true;
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = Color.yellow;
            }
            yield return new WaitForSeconds(config.skirmisherWindup);
            if (state != EnemyState.Dead && player != null
                && Vector3.Distance(transform.position, player.position) <= config.skirmisherAttackRange)
            {
                playerCombat.ReceiveEnemyAttack(config.skirmisherDamage);
            }
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = baseColor;
            }
            windingUp = false;
        }

        private void Die()
        {
            state = EnemyState.Dead;
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            waveManager?.EnemyDied(this);
            Destroy(gameObject);
        }
    }
}

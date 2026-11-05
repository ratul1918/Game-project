using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace TheLightHouse
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ListenerAI : MonoBehaviour, IDamageable
    {
        public enum ListenerState
        {
            Lurk,
            InvestigateNoise,
            AggressiveCharge,
            Attack,
            Staggered
        }

        [Header("Hearing Settings")]
        [SerializeField] private float hearingRadius = 25f;
        [SerializeField] private float chargeNoiseThreshold = 0.5f;

        [Header("Speed Settings")]
        [SerializeField] private float lurkSpeed = 1.4f;
        [SerializeField] private float chargeSpeed = 5.2f;

        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackCooldown = 1.6f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 80f;
        [SerializeField] private float currentHealth = 80f;

        private NavMeshAgent agent;
        private ListenerState state = ListenerState.Lurk;
        private Transform playerTransform;
        private PlayerHealthState playerHealth;
        private Vector3 targetSoundPosition;
        private float nextAttackTime;
        private float searchTimer;

        public ListenerState CurrentState => state;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            currentHealth = maxHealth;
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                playerTransform = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealthState>();
            }
        }

        private void OnEnable()
        {
            GameEvents.NoiseHeard += OnNoiseHeard;
        }

        private void OnDisable()
        {
            GameEvents.NoiseHeard -= OnNoiseHeard;
        }

        private void Update()
        {
            if (state == ListenerState.Staggered) return;

            switch (state)
            {
                case ListenerState.Lurk:
                    UpdateLurk();
                    break;
                case ListenerState.InvestigateNoise:
                    UpdateInvestigate();
                    break;
                case ListenerState.AggressiveCharge:
                    UpdateCharge();
                    break;
                case ListenerState.Attack:
                    UpdateAttack();
                    break;
            }
        }

        private void UpdateLurk()
        {
            agent.speed = lurkSpeed;
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                // Wander slowly
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * 6f;
                if (NavMesh.SamplePosition(randomPoint, out var hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }

        private void UpdateInvestigate()
        {
            agent.speed = lurkSpeed * 1.5f;
            if (agent.remainingDistance <= 0.6f)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= 3.5f)
                {
                    state = ListenerState.Lurk;
                }
            }
        }

        private void UpdateCharge()
        {
            agent.speed = chargeSpeed;

            // If player is close, attack
            if (playerTransform && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                state = ListenerState.Attack;
                return;
            }

            if (agent.remainingDistance <= 0.8f)
            {
                searchTimer = 0f;
                state = ListenerState.InvestigateNoise;
            }
        }

        private void UpdateAttack()
        {
            agent.isStopped = true;
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                if (playerHealth && Vector3.Distance(transform.position, playerTransform.position) <= attackRange + 0.5f)
                {
                    playerHealth.TakeDamage(attackDamage, causeBleeding: true);
                }
            }

            if (playerTransform && Vector3.Distance(transform.position, playerTransform.position) > attackRange + 0.8f)
            {
                agent.isStopped = false;
                state = ListenerState.AggressiveCharge;
            }
        }

        private void OnNoiseHeard(NoiseEmittedEvent e)
        {
            if (state == ListenerState.Staggered) return;
            if (e.Source == gameObject) return;

            float distance = Vector3.Distance(transform.position, e.Position);
            if (distance > hearingRadius) return;

            targetSoundPosition = e.Position;

            if (e.Loudness >= chargeNoiseThreshold)
            {
                state = ListenerState.AggressiveCharge;
                agent.isStopped = false;
                agent.SetDestination(targetSoundPosition);
            }
            else if (state == ListenerState.Lurk)
            {
                state = ListenerState.InvestigateNoise;
                searchTimer = 0f;
                agent.isStopped = false;
                agent.SetDestination(targetSoundPosition);
            }
        }

        public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection, bool isHeavy)
        {
            currentHealth -= damage;
            StartCoroutine(StaggerRoutine(isHeavy ? 1.6f : 0.6f, hitDirection));

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private IEnumerator StaggerRoutine(float duration, Vector3 hitDirection)
        {
            state = ListenerState.Staggered;
            agent.isStopped = true;

            // Push back slightly
            Vector3 push = hitDirection.normalized * 1.5f;
            push.y = 0;
            transform.position += push;

            yield return new WaitForSeconds(duration);

            agent.isStopped = false;
            state = ListenerState.AggressiveCharge;
            if (playerTransform) agent.SetDestination(playerTransform.position);
        }

        private void Die()
        {
            agent.isStopped = true;
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}

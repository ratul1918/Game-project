using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace TheLightHouse
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class GraspedAI : MonoBehaviour, IDamageable
    {
        public enum GraspedState
        {
            Patrol,
            Pursue,
            Grapple,
            Staggered
        }

        [Header("Movement & Range")]
        [SerializeField] private float walkSpeed = 1.6f;
        [SerializeField] private float pursueSpeed = 3.2f;
        [SerializeField] private float grappleRange = 1.8f;
        [SerializeField] private float crushDamage = 45f;
        [SerializeField] private float struggleTimeLimit = 2.4f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 120f;
        [SerializeField] private float currentHealth = 120f;

        private NavMeshAgent agent;
        private GraspedState state = GraspedState.Patrol;
        private Transform playerTransform;
        private FirstPersonController playerController;
        private PlayerHealthState playerHealth;
        private Coroutine grappleRoutine;

        public GraspedState CurrentState => state;
        public bool IsGrappling => state == GraspedState.Grapple;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            currentHealth = maxHealth;
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                playerTransform = playerObj.transform;
                playerController = playerObj.GetComponent<FirstPersonController>();
                playerHealth = playerObj.GetComponent<PlayerHealthState>();
            }
        }

        private void Update()
        {
            if (state == GraspedState.Staggered || state == GraspedState.Grapple) return;

            if (playerTransform)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist <= grappleRange)
                {
                    StartGrapple();
                }
                else if (dist <= 12f)
                {
                    state = GraspedState.Pursue;
                    agent.speed = pursueSpeed;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    state = GraspedState.Patrol;
                    agent.speed = walkSpeed;
                }
            }
        }

        private void StartGrapple()
        {
            state = GraspedState.Grapple;
            agent.isStopped = true;

            if (playerController)
            {
                playerController.SetMovementLocked(true);
            }

            grappleRoutine = StartCoroutine(GrappleCountdown());
        }

        private IEnumerator GrappleCountdown()
        {
            float elapsed = 0f;
            while (elapsed < struggleTimeLimit)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Time ran out: deliver crush damage
            if (playerHealth)
            {
                playerHealth.TakeDamage(crushDamage, causeBleeding: true);
            }

            ReleaseGrapple(false);
        }

        public void BreakGrappleByStruggle()
        {
            if (state != GraspedState.Grapple) return;
            ReleaseGrapple(true);
        }

        private void ReleaseGrapple(bool staggered)
        {
            if (grappleRoutine != null)
            {
                StopCoroutine(grappleRoutine);
                grappleRoutine = null;
            }

            if (playerController)
            {
                playerController.SetMovementLocked(false);
            }

            if (staggered)
            {
                StartCoroutine(StaggerRoutine(2.0f, -transform.forward));
            }
            else
            {
                state = GraspedState.Pursue;
                agent.isStopped = false;
            }
        }

        public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection, bool isHeavy)
        {
            currentHealth -= damage;

            if (state == GraspedState.Grapple)
            {
                ReleaseGrapple(true);
                return;
            }

            StartCoroutine(StaggerRoutine(isHeavy ? 1.8f : 0.8f, hitDirection));

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private IEnumerator StaggerRoutine(float duration, Vector3 hitDirection)
        {
            state = GraspedState.Staggered;
            agent.isStopped = true;

            Vector3 push = hitDirection.normalized * 1.8f;
            push.y = 0;
            transform.position += push;

            yield return new WaitForSeconds(duration);

            agent.isStopped = false;
            state = GraspedState.Pursue;
        }

        private void Die()
        {
            if (state == GraspedState.Grapple && playerController)
            {
                playerController.SetMovementLocked(false);
            }
            agent.isStopped = true;
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}

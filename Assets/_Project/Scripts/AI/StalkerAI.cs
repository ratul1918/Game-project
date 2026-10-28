using UnityEngine;
using UnityEngine.AI;

namespace TheLightHouse
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class StalkerAI : MonoBehaviour
    {
        public enum StalkerState
        {
            Idle,
            Investigate,
            Search
        }

        [Header("Hearing")]
        [SerializeField, Min(0f)] private float hearingRange = 30f;

        [Header("Investigation")]
        [SerializeField, Min(0f)] private float arrivalDistance = 0.35f;
        [SerializeField, Min(0f)] private float searchDuration = 4f;
        [SerializeField, Min(0f)] private float searchTurnSpeed = 90f;

        private NavMeshAgent agent;
        private StalkerState currentState = StalkerState.Idle;
        private float searchTimeRemaining;
        private bool reportedMissingNavMesh;

        public StalkerState CurrentState => currentState;
        public string StateName => currentState.ToString();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            GameEvents.NoiseEmitted += HandleNoiseEmitted;
        }

        private void Start()
        {
            Debug.Log($"Stalker state: {currentState}", this);
        }

        private void OnDisable()
        {
            GameEvents.NoiseEmitted -= HandleNoiseEmitted;
        }

        private void Update()
        {
            switch (currentState)
            {
                case StalkerState.Investigate:
                    UpdateInvestigation();
                    break;
                case StalkerState.Search:
                    UpdateSearch();
                    break;
            }
        }

        private void HandleNoiseEmitted(NoiseEmittedEvent eventData)
        {
            float audibleRadius = Mathf.Min(hearingRange, eventData.Loudness);
            float distanceToNoise = Vector3.Distance(transform.position, eventData.Position);
            if (distanceToNoise > audibleRadius)
            {
                return;
            }

            ChangeState(StalkerState.Investigate);

            if (agent.isOnNavMesh)
            {
                reportedMissingNavMesh = false;
                agent.isStopped = false;
                agent.SetDestination(eventData.Position);
            }
            else if (!reportedMissingNavMesh)
            {
                reportedMissingNavMesh = true;
                Debug.LogWarning(
                    "Stalker heard a noise but is not on a baked NavMesh. Bake the scene's NavMeshSurface.",
                    this);
            }
        }

        private void UpdateInvestigation()
        {
            if (!agent.isOnNavMesh || agent.pathPending)
            {
                return;
            }

            float stoppingDistance = Mathf.Max(agent.stoppingDistance, arrivalDistance);
            if (!agent.hasPath || agent.remainingDistance > stoppingDistance)
            {
                return;
            }

            agent.isStopped = true;
            searchTimeRemaining = searchDuration;
            ChangeState(StalkerState.Search);
        }

        private void UpdateSearch()
        {
            transform.Rotate(Vector3.up, searchTurnSpeed * Time.deltaTime, Space.World);
            searchTimeRemaining -= Time.deltaTime;
            if (searchTimeRemaining > 0f)
            {
                return;
            }

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }

            ChangeState(StalkerState.Idle);
        }

        private void ChangeState(StalkerState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            Debug.Log($"Stalker state: {currentState}", this);
        }
    }
}

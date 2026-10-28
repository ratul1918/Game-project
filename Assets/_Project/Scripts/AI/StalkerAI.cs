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
            Search,
            Patrol,
            Hunt,
            CheckHidingSpot,
            Retreat
        }

        [Header("Hearing")]
        [SerializeField, Min(0f)] private float hearingRange = 30f;

        [Header("Investigation")]
        [SerializeField, Min(0f)] private float arrivalDistance = 0.35f;
        [SerializeField, Min(0f)] private float searchDuration = 4f;
        [SerializeField, Min(0f)] private float searchTurnSpeed = 90f;

        [Header("Act 2")]
        [SerializeField] private bool actTwo;
        [SerializeField] private PlayerHealthState player;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private HidingSpot[] hidingSpots;
        [SerializeField] private AnimationCurve hearingCurve = AnimationCurve.Linear(0f, 1f, 1f, .5f);
        [SerializeField] private float closePresenceRadius = 1.2f;
        [SerializeField] private float catchDistance = .8f;
        [SerializeField] private float hearingCooldown = .6f;
        [SerializeField] private float searchRadius = 3f;
        [SerializeField] private int searchPointCount = 3;
        [SerializeField, Range(0, 1)] private float hidingCheckChance = .25f;
        [SerializeField] private float sampleRadius = 3f;
        private float nextHearingTime;
        private int waypointIndex;
        private int searchedPoints;
        private bool playerHidden;
        private bool announcedFirstNoise;
        private HidingSpot checkingSpot;
        private Vector3 searchOrigin;
        private CraneHerding herding;
        private NavMeshAgent agent;
        private StalkerState currentState = StalkerState.Idle;
        private float searchTimeRemaining;
        private bool reportedMissingNavMesh;

        public StalkerState CurrentState => currentState;
        public string StateName => currentState.ToString();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            herding = GetComponent<CraneHerding>();
        }

        private void OnEnable()
        {
            GameEvents.NoiseEmitted += HandleNoiseEmitted;
            GameEvents.PlayerHidden += Hidden;
            GameEvents.PlayerRespawned += ResetPatrol;
        }

        private void Start()
        {
            if (actTwo) ResetPatrol();
            Debug.Log($"Stalker state: {currentState}", this);
        }

        private void OnDisable()
        {
            GameEvents.NoiseEmitted -= HandleNoiseEmitted;
            GameEvents.PlayerHidden -= Hidden;
            GameEvents.PlayerRespawned -= ResetPatrol;
        }

        private void Update()
        {
            if (actTwo && player && !player.IsCaught && !playerHidden &&
                Vector3.Distance(transform.position, player.transform.position) <= closePresenceRadius)
            { searchOrigin=player.transform.position; ChangeState(StalkerState.Hunt); }
            if (actTwo && currentState == StalkerState.Hunt)
            {
                if (playerHidden) { searchOrigin = transform.position; BeginSearch(); return; }
                if (player && !player.IsCaught)
                {
                    MoveTo(searchOrigin);
                    if (Vector3.Distance(transform.position, player.transform.position) <= catchDistance)
                    {
                        GameEvents.RaisePlayerSpotted(new("crane-stalker", player.transform.position));
                        GameEvents.RaiseCatchRequested();
                        ChangeState(StalkerState.Retreat);
                    }
                    else if (Arrived()) BeginSearch();
                }
                return;
            }
            if (actTwo && currentState == StalkerState.Retreat) return;
            if (actTwo && currentState == StalkerState.Patrol)
            {
                if (Arrived()) NextPatrolPoint();
                return;
            }
            if (actTwo && currentState == StalkerState.CheckHidingSpot)
            {
                if (Arrived())
                {
                    if (checkingSpot && checkingSpot.Occupied)
                    {
                        GameEvents.RaiseHidingExited();
                        GameEvents.RaisePlayerSpotted(new("crane-stalker", player.transform.position));
                        GameEvents.RaiseCatchRequested(); ChangeState(StalkerState.Retreat);
                    }
                    else ResetPatrol();
                }
                return;
            }
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
            if (eventData.Source == gameObject || (actTwo && (Time.time < nextHearingTime || currentState == StalkerState.Retreat))) return;
            float audibleRadius = Mathf.Min(hearingRange, eventData.Loudness);
            float distanceToNoise = Vector3.Distance(transform.position, eventData.Position);
            if (actTwo) audibleRadius *= Mathf.Clamp01(hearingCurve.Evaluate(distanceToNoise / Mathf.Max(hearingRange, .01f)));
            if (distanceToNoise > audibleRadius)
            {
                return;
            }

            nextHearingTime = Time.time + hearingCooldown;
            searchOrigin = eventData.Position;
            if (actTwo && !announcedFirstNoise) { announcedFirstNoise=true; GameEvents.RaiseStorySignal("stalker-noise"); }
            ChangeState(actTwo && eventData.Source == (player ? player.gameObject : null) && !playerHidden
                ? StalkerState.Hunt : StalkerState.Investigate);

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
            if (agent.hasPath ? agent.remainingDistance > stoppingDistance :
                Vector3.Distance(transform.position, searchOrigin) > stoppingDistance)
            {
                return;
            }

            BeginSearch();
        }

        private void UpdateSearch()
        {
            if (actTwo && searchedPoints < searchPointCount && Arrived())
            {
                searchedPoints++;
                Vector2 offset = Random.insideUnitCircle * searchRadius;
                Vector3 destination = searchOrigin + new Vector3(offset.x, 0f, offset.y);
                MoveTo(herding ? herding.Bias(destination) : destination);
            }
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

            if (actTwo)
            {
                foreach (var spot in hidingSpots)
                {
                    if (!spot || Vector3.Distance(transform.position,spot.transform.position)>searchRadius || Random.value>hidingCheckChance) continue;
                    checkingSpot=spot; ChangeState(StalkerState.CheckHidingSpot); MoveTo(spot.transform.position); return;
                }
                ResetPatrol(); return;
            }
            ChangeState(StalkerState.Idle);
        }

        void Hidden(PlayerHiddenEvent e) => playerHidden=e.IsHidden;
        void BeginSearch()
        {
            searchTimeRemaining=searchDuration;searchedPoints=0;
            if(agent.isOnNavMesh) agent.isStopped=true;
            ChangeState(StalkerState.Search);
        }
        bool Arrived() => agent.isOnNavMesh && !agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= Mathf.Max(arrivalDistance, agent.stoppingDistance));
        void MoveTo(Vector3 point)
        {
            if(!agent.isOnNavMesh) return;
            if(NavMesh.SamplePosition(point,out NavMeshHit hit,sampleRadius,agent.areaMask))
            { agent.isStopped=false;agent.SetDestination(hit.position); }
        }
        void NextPatrolPoint()
        {
            if(waypoints==null||waypoints.Length==0)return;
            var waypoint=waypoints[waypointIndex++%waypoints.Length];
            if(waypoint)MoveTo(herding?herding.Bias(waypoint.position):waypoint.position);
        }
        void ResetPatrol()
        {
            if(!actTwo)return;
            playerHidden=player&&player.IsHidden;
            nextHearingTime=Time.time+hearingCooldown;
            ChangeState(StalkerState.Patrol);NextPatrolPoint();
        }
        private void ChangeState(StalkerState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            GameEvents.RaiseStalkerStateChanged(currentState);
            Debug.Log($"Stalker state: {currentState}", this);
        }
    }
}

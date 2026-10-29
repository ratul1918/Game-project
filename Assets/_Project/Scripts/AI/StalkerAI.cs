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
            Retreat,
            Climb
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
        [SerializeField] private StormSettings stormSettings;
        [SerializeField] private AudioClip stairFootsteps;
        private bool stairWarned;
        private float resumeClimbAt;
        private bool playerCranking;
        private bool stormComplete;
        [Header("Act 4")]
        [SerializeField] private bool postRevealMode;
        [SerializeField] private ActFourSettings actFourSettings;
        [SerializeField] private AudioClip lullabyHum;
        private float baseSpeed;
        private float retreatUntil;
        private bool hummed;
        private float readingSpeed=1f;
        private void StormBeat(StoryBeatTriggeredEvent e)
        {
            if(e.Id=="A3-B09"&&stormSettings){stormComplete=true;if(agent.isOnNavMesh){agent.ResetPath();agent.isStopped=true;}ChangeState(StalkerState.Idle);}
            if(e.Id=="A4-B01"){stormComplete=false;ResetPatrol();}
            if(e.Id=="A4-B05")postRevealMode=true;
        }
        private void CrankActive(bool active)=>playerCranking=active;
        private StalkerState currentState = StalkerState.Idle;
        private float searchTimeRemaining;
        private bool reportedMissingNavMesh;

        public StalkerState CurrentState => currentState;
        public string StateName => currentState.ToString();
        public bool PostRevealMode => postRevealMode;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            herding = GetComponent<CraneHerding>();
            baseSpeed=agent.speed;
        }

        private void OnEnable()
        {
            GameEvents.NoiseHeard += HandleNoiseEmitted;
            GameEvents.PlayerHidden += Hidden;
            GameEvents.PlayerRespawned += ResetPatrol;
            GameEvents.CrankActiveChanged += CrankActive;
            GameEvents.StoryBeatTriggered += StormBeat;
            GameEvents.ReadingStateChanged += Reading;
        }

        private void Start()
        {
            if (actTwo) ResetPatrol();
            Debug.Log($"Stalker state: {currentState}", this);
        }

        private void OnDisable()
        {
            GameEvents.NoiseHeard -= HandleNoiseEmitted;
            GameEvents.PlayerHidden -= Hidden;
            GameEvents.PlayerRespawned -= ResetPatrol;
            GameEvents.CrankActiveChanged -= CrankActive;
            GameEvents.StoryBeatTriggered -= StormBeat;
            GameEvents.ReadingStateChanged -= Reading;
        }

        private void Reading(ReadingStateChangedEvent e){readingSpeed=e.IsReading?e.ThreatSpeed:1f;if(agent)agent.speed=baseSpeed*readingSpeed;}

        private void Update()
        {
            if(stormComplete)return;
            if(postRevealMode&&currentState==StalkerState.Retreat&&Time.time>=retreatUntil){ResetPatrol();return;}
            if(postRevealMode&&player&&!hummed&&Vector3.Distance(transform.position,player.transform.position)<= (actFourSettings?actFourSettings.lullabyRange:9f))
            {
                hummed=true;GameEvents.RequestDialogue(new("","(Crane hums a broken lullaby)"));
                if(lullabyHum&&TryGetComponent<AudioSource>(out var hum)){hum.loop=true;hum.volume=actFourSettings?actFourSettings.lullabyVolume:.45f;hum.clip=lullabyHum;hum.Play();}
            }
            if(stormSettings&&currentState==StalkerState.Climb)
            {
                if(!stairWarned&&transform.position.y>=stormSettings.stairY)
                {
                    stairWarned=true;resumeClimbAt=Time.time+stormSettings.stairPause;
                    if(agent.isOnNavMesh)agent.isStopped=true;
                    GameEvents.RequestDialogue(new("","(heavy footsteps on the stairs)"));
                    if(stairFootsteps&&TryGetComponent<AudioSource>(out var audio))audio.PlayOneShot(stairFootsteps);
                    GameEvents.RaiseNoiseEmitted(new(transform.position,6f,gameObject));
                    GameEvents.RaiseStorySignal("stalker-stairs");
                }
                if(Time.time<resumeClimbAt)return;
                if(agent.isOnNavMesh)agent.isStopped=false;
                if(Arrived())
                {
                    if(playerCranking&&player&&!playerHidden)
                    {
                        ReachPlayer();
                    }
                    else BeginSearch();
                }
                return;
            }
            if (actTwo && player && !player.IsCaught && !playerHidden &&
                Vector3.Distance(transform.position, player.transform.position) <= closePresenceRadius)
            { searchOrigin=player.transform.position; ChangeState(StalkerState.Hunt); }
            if (actTwo && currentState == StalkerState.Hunt)
            {
                if (playerHidden) { searchOrigin = transform.position; BeginSearch(); return; }
                if (player && !player.IsCaught)
                {
                    MoveTo(searchOrigin);
                    float contact=postRevealMode&&actFourSettings?actFourSettings.postRevealStopDistance:catchDistance;
                    if (Vector3.Distance(transform.position, player.transform.position) <= contact)
                    {
                        ReachPlayer();
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
                        if(postRevealMode){GameEvents.RaiseHidingExited();ReachPlayer();}
                        else {GameEvents.RaiseHidingExited();GameEvents.RaisePlayerSpotted(new("crane-stalker", player.transform.position));GameEvents.RaiseCatchRequested();ChangeState(StalkerState.Retreat);}
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
            if(stormComplete)return;
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
            if(stormSettings&&eventData.Source&&eventData.Source.GetComponent<CrankStation>()&&
                eventData.Position.y>=stormSettings.lanternY&&transform.position.y<stormSettings.lanternY)
            {
                if(currentState==StalkerState.Climb&&Time.time<resumeClimbAt)return;
                if(currentState!=StalkerState.Climb){stairWarned=false;resumeClimbAt=0;}
                ChangeState(StalkerState.Climb);MoveTo(searchOrigin);return;
            }
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
                    float chance=stormSettings&&transform.position.y>=stormSettings.lanternY?stormSettings.lanternHidingCheckChance:hidingCheckChance;
                    if (!spot || Vector3.Distance(transform.position,spot.transform.position)>searchRadius || Random.value>chance) continue;
                    checkingSpot=spot; ChangeState(StalkerState.CheckHidingSpot); MoveTo(spot.transform.position); return;
                }
                ResetPatrol(); return;
            }
            ChangeState(StalkerState.Idle);
        }

        void Hidden(PlayerHiddenEvent e) => playerHidden=e.IsHidden;
        void BeginSearch()
        {
            searchTimeRemaining=stormSettings&&transform.position.y>=stormSettings.lanternY?stormSettings.lanternSearchTime:searchDuration;searchedPoints=0;
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
            if(stormSettings&&waypoint&&waypoint.position.y>=stormSettings.lanternY&&transform.position.y<stormSettings.lanternY)
            {
                stairWarned=false;resumeClimbAt=0;searchOrigin=waypoint.position;
                ChangeState(StalkerState.Climb);MoveTo(searchOrigin);return;
            }
            if(waypoint)MoveTo(herding?herding.Bias(waypoint.position):waypoint.position);
        }
        void ResetPatrol()
        {
            if(!actTwo)return;
            playerHidden=player&&player.IsHidden;
            nextHearingTime=Time.time+hearingCooldown;
            ChangeState(StalkerState.Patrol);NextPatrolPoint();
        }
        void ReachPlayer()
        {
            if(postRevealMode)
            {
                if(agent.isOnNavMesh){agent.ResetPath();agent.isStopped=true;}
                GameEvents.RequestDialogue(new("","(he stops, humming)"));
                retreatUntil=Time.time+(actFourSettings?actFourSettings.postRevealPause:2.5f);ChangeState(StalkerState.Retreat);return;
            }
            GameEvents.RaisePlayerSpotted(new("crane-stalker",player.transform.position));GameEvents.RaiseCatchRequested();ChangeState(StalkerState.Retreat);
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

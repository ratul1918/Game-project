using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using EmberBlade.Combat;
using EmberBlade.Missions;

namespace EmberBlade.Network
{
    public class MissionApi : MonoBehaviour
    {
        public static MissionApi Instance { get; private set; }

        [SerializeField] private string baseUrl = "http://localhost:8080";
        [SerializeField] private float storyBeatDuration = 5f;

        private string token;
        private MissionManager missionManager;
        private TMP_Text storyBeatText;
        private Coroutine hideStoryBeat;

        public static MissionApi Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            return new GameObject("MissionApi").AddComponent<MissionApi>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        public void Configure(string jwtToken)
        {
            token = jwtToken;
            BindToScene(SceneManager.GetActiveScene());
            StartCoroutine(LoadProgress());
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindToScene(scene);
        }

        private void BindToScene(Scene scene)
        {
            if (missionManager != null)
            {
                missionManager.OnMissionStarted -= HandleMissionStarted;
                missionManager.OnMissionCompleted -= HandleMissionCompleted;
            }

            missionManager = FindAnyObjectByType<MissionManager>();
            storyBeatText = GameObject.Find("StoryBeatText")?.GetComponent<TMP_Text>();
            if (missionManager != null)
            {
                missionManager.OnMissionStarted += HandleMissionStarted;
                missionManager.OnMissionCompleted += HandleMissionCompleted;
            }

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.StoryBeatReceived -= HandleStoryBeat;
                NetworkManager.Instance.StoryBeatReceived += HandleStoryBeat;
            }
        }

        private void HandleMissionStarted(string missionId)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                StartCoroutine(PostJson(
                    "/api/missions/start",
                    $"{{\"missionId\":\"{missionId}\"}}"));
            }
        }

        private void HandleMissionCompleted(string missionId, bool success)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            string json = JsonUtility.ToJson(new MissionResult(
                missionId,
                success,
                BarricadeResult.WallsPlaced,
                BarricadeResult.TrapsPlaced,
                Time.timeSinceLevelLoad));
            StartCoroutine(PostJson("/api/missions/result", json));
        }

        private IEnumerator LoadProgress()
        {
            yield return Request("/api/save", UnityWebRequest.kHttpVerbGET, null);
        }

        private IEnumerator PostJson(string path, string json)
        {
            yield return Request(path, UnityWebRequest.kHttpVerbPOST, json);
        }

        private IEnumerator Request(string path, string method, string json)
        {
            using UnityWebRequest request = new UnityWebRequest(baseUrl + path, method);
            if (json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(
                    System.Text.Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Mission API request {path} failed: {request.error}");
                yield break;
            }

            if (path == "/api/save")
            {
                Debug.Log($"Loaded Ember Blade save progress: {request.downloadHandler.text}");
            }
        }

        private void HandleStoryBeat(NetworkManager.StoryBeat beat)
        {
            if (storyBeatText == null)
            {
                return;
            }

            storyBeatText.text = $"Chapter {beat.chapter} — {beat.mission} {beat.beat}";
            storyBeatText.gameObject.SetActive(true);
            if (hideStoryBeat != null)
            {
                StopCoroutine(hideStoryBeat);
            }
            hideStoryBeat = StartCoroutine(HideStoryBeat());
        }

        private IEnumerator HideStoryBeat()
        {
            yield return new WaitForSeconds(storyBeatDuration);
            if (storyBeatText != null)
            {
                storyBeatText.gameObject.SetActive(false);
            }
            hideStoryBeat = null;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (missionManager != null)
            {
                missionManager.OnMissionStarted -= HandleMissionStarted;
                missionManager.OnMissionCompleted -= HandleMissionCompleted;
            }
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.StoryBeatReceived -= HandleStoryBeat;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        [Serializable]
        private struct MissionResult
        {
            public string missionId;
            public bool success;
            public int barricadeWalls;
            public int barricadeTraps;
            public float timeSeconds;

            public MissionResult(
                string id, bool completed, int walls, int traps, float seconds)
            {
                missionId = id;
                success = completed;
                barricadeWalls = walls;
                barricadeTraps = traps;
                timeSeconds = seconds;
            }
        }
    }
}

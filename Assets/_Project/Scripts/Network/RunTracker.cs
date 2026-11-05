using System;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class RunTracker : MonoBehaviour
    {
        [SerializeField] CloudSaveClient cloud;
        int deaths;
        float startedAt;
        string runId;
        void Awake() { DontDestroyOnLoad(gameObject); if (!cloud) cloud = FindAnyObjectByType<CloudSaveClient>(); }
        void OnEnable() { GameEvents.CatchRequested += Death; GameEvents.GameStateChanged += State; Application.quitting += Quit; }
        void OnDisable() { GameEvents.CatchRequested -= Death; GameEvents.GameStateChanged -= State; Application.quitting -= Quit; }
        public void StartRun() { deaths = 0; startedAt = Time.realtimeSinceStartup; cloud?.StartRun(value => runId = value); }
        void Death() { if (!string.IsNullOrEmpty(runId)) deaths++; }
        void State(GameStateChangedEvent e) { if (e.Current == GameState.Ending) End("completed"); }
        void Quit() { if (!string.IsNullOrEmpty(runId)) End("quit"); }
        void End(string outcome)
        {
            int duration = Mathf.Max(0, Mathf.RoundToInt(Time.realtimeSinceStartup - startedAt));
            cloud?.EndRun(runId, outcome, deaths, duration); runId = null;
        }
    }
}

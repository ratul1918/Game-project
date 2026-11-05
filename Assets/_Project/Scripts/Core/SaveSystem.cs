using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheLightHouse
{
    [Serializable] public sealed class GameOptionsData
    {
        // General / Display
        public bool reducedStorm = true;
        public bool disableBobAndFov;
        public float sensitivity = 1f;
        public float volume = 1f;
        [Range(.7f, 1f)] public float renderScale = 1f;
        public bool telemetryEnabled = true;
        public bool vSync = true;
        public int frameRateLimit = 60;
        public int qualityPreset = 0; // 0: Low/Mac, 1: Medium, 2: High
        public int shadowQuality = 1;
        public int textureQuality = 0;
        public float fov = 65f;
        public float brightness = 1f;

        // Audio Channels
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float ambienceVolume = 0.9f;
        public float sfxVolume = 1f;
        public float dialogueVolume = 1f;
        public float footstepsVolume = 0.85f;
        public float horrorVolume = 1f;

        // Gameplay
        public bool invertY;
        public float headBobIntensity = 1f;
        public bool cameraBob = true;
        public bool motionSicknessReduction;
        public bool objectiveAssistance = true;
        public int difficulty = 1; // 0: Story, 1: Survival, 2: Nightmare

        // Accessibility
        public bool subtitlesEnabled = true;
        public float subtitleSize = 1f;
        public float subtitleBackgroundOpacity = 0.75f;
        public bool reducedFlashing;
        public bool reducedCameraShake;
        public bool toggleInteraction;
    }
    [Serializable] public sealed class GameSaveData
    {
        public string act = "Act1";
        public string checkpointId = "start";
        public Vector3 checkpoint;
        public List<string> items = new();
        public List<string> pages = new();
        public List<string> flags = new();
        public GameOptionsData options = new();
        public int version = -1;
        public string updatedAt;
        public int slotIndex = 1;
        public int highestUnlockedAct = 1;
    }

    public sealed class SaveSystem : MonoBehaviour
    {
        [SerializeField] string fileName = "thelighthouse-save.json";
        [SerializeField] int currentSlot = 1;
        GameSaveData data = new();
        HashSet<string> items = new(), pages = new(), flags = new();
        CloudSaveClient cloud;
        CloudSavePayload pendingCloud;
        public GameSaveData Data => data;
        public int CurrentSlot => currentSlot;
        public int HighestUnlockedAct => Mathf.Max(1, data.highestUnlockedAct);

        string GetSlotFileName(int slot) => slot == 1 ? fileName : $"thelighthouse-save-slot{slot}.json";
        public bool HasSave => File.Exists(Path.Combine(Application.persistentDataPath, GetSlotFileName(currentSlot))) || File.Exists(Path.Combine(Application.persistentDataPath, fileName));
        public bool HasSlotSave(int slot) => File.Exists(Path.Combine(Application.persistentDataPath, GetSlotFileName(slot)));

        void Awake()
        {
            var existing = FindObjectsByType<SaveSystem>();
            if (existing.Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            cloud = FindAnyObjectByType<CloudSaveClient>();
        }

        void OnEnable()
        {
            GameEvents.ItemCollected += Item;
            GameEvents.PageRead += Page;
            GameEvents.StoryBeatTriggered += Beat;
            GameEvents.CheckpointReached += Checkpoint;
        }

        void OnDisable()
        {
            GameEvents.ItemCollected -= Item;
            GameEvents.PageRead -= Page;
            GameEvents.StoryBeatTriggered -= Beat;
            GameEvents.CheckpointReached -= Checkpoint;
        }

        void Item(ItemCollectedEvent e) => items.Add(e.Id);
        void Page(PageReadEvent e) => pages.Add(e.Id);
        void Beat(StoryBeatTriggeredEvent e)
        {
            flags.Add(e.Id);
            int unlocked = e.Id switch
            {
                "A1-B06" => 2,
                "A2-B07" => 3,
                "A3-B09" => 4,
                "A4-B06" => 5,
                _ => 1
            };
            if (unlocked > data.highestUnlockedAct)
            {
                data.highestUnlockedAct = unlocked;
                Save();
            }
        }

        void Checkpoint(CheckpointReachedEvent e)
        {
            data.checkpointId = e.Id;
            data.checkpoint = e.Position;
            data.act = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Save();
        }

        public void SetCurrentSlot(int slot)
        {
            currentSlot = Mathf.Clamp(slot, 1, 3);
            Load();
        }

        public void Save()
        {
            data.items = new(items);
            data.pages = new(pages);
            data.flags = new(flags);
            data.slotIndex = currentSlot;
            data.version++;
            data.updatedAt = DateTime.UtcNow.ToString("O");
            string path = Path.Combine(Application.persistentDataPath, GetSlotFileName(currentSlot));
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            PushCloud();
        }
        void PushCloud()
        {
            cloud ??=FindAnyObjectByType<CloudSaveClient>(); if(!cloud)return;
            pendingCloud=new CloudSavePayload{act=data.act,checkpoint=data.checkpointId,itemsJson=JsonUtility.ToJson(new StringList{values=data.items}),pagesJson=JsonUtility.ToJson(new StringList{values=data.pages}),flagsJson=JsonUtility.ToJson(new StringList{values=data.flags}),optionsJson=JsonUtility.ToJson(data.options),version=data.version,updatedAt=data.updatedAt};
            cloud.PutSave(pendingCloud,(remote,error)=>{if(error!=null)StartCoroutine(RetryCloud());else pendingCloud=null;});
        }
        System.Collections.IEnumerator RetryCloud(){yield return new WaitForSecondsRealtime(5f);if(pendingCloud!=null)cloud?.PutSave(pendingCloud,(remote,error)=>{if(error!=null)StartCoroutine(RetryCloud());else pendingCloud=null;});}
        public bool Load()
        {
            string path = Path.Combine(Application.persistentDataPath, GetSlotFileName(currentSlot));
            if (!File.Exists(path))
            {
                path = Path.Combine(Application.persistentDataPath, fileName);
                if (!File.Exists(path)) return false;
            }
            data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path)) ?? new GameSaveData();
            items = new(data.items ?? new());
            pages = new(data.pages ?? new());
            flags = new(data.flags ?? new());
            cloud ??= FindAnyObjectByType<CloudSaveClient>();
            cloud?.GetSave((remote, error) => { if (remote != null) MergeCloud(remote); });
            return true;
        }
        void MergeCloud(CloudSavePayload remote)
        {
            bool useCloud=false;
            if(DateTime.TryParse(remote.updatedAt,out var cloudTime)&&DateTime.TryParse(data.updatedAt,out var localTime))useCloud=cloudTime>localTime;
            else useCloud=remote.version>data.version;
            Debug.Log("Continue save choice: "+(useCloud?"cloud":"local"));
            if(!useCloud)return;
            data.act=remote.act;data.checkpointId=remote.checkpoint;data.version=remote.version;data.updatedAt=remote.updatedAt;
            data.items=JsonUtility.FromJson<StringList>(remote.itemsJson)?.values??new List<string>();
            data.pages=JsonUtility.FromJson<StringList>(remote.pagesJson)?.values??new List<string>();
            data.flags=JsonUtility.FromJson<StringList>(remote.flagsJson)?.values??new List<string>();
            data.options=JsonUtility.FromJson<GameOptionsData>(remote.optionsJson)??new GameOptionsData();
            items=new(data.items);pages=new(data.pages);flags=new(data.flags);
        }
        [Serializable] sealed class StringList { public List<string> values; }
        public void NewGame(){var currentOptions=data.options;data=new GameSaveData{options=currentOptions??new GameOptionsData()};items.Clear();pages.Clear();flags.Clear();Save();}
        public void SetOptions(GameOptionsData value){data.options=value??new GameOptionsData();Save();}
        public void RestoreSceneState()
        {
            var inventory=FindAnyObjectByType<Inventory>();inventory?.Restore(items);
            var journal=FindAnyObjectByType<LogbookJournal>();journal?.Restore(pages);
            var health=FindAnyObjectByType<PlayerHealthState>();if(health&&data.checkpoint!=Vector3.zero)health.SetCheckpoint(data.checkpoint);
            var controller=FindAnyObjectByType<FirstPersonController>();controller?.ApplyOptions(data.options.sensitivity,data.options.disableBobAndFov);
            var accessibility=FindAnyObjectByType<AccessibilityOptions>();accessibility?.SetReduced(data.options.reducedStorm);
            AudioListener.volume=data.options.volume;
            if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipeline)pipeline.renderScale=Mathf.Clamp(data.options.renderScale,.7f,1f);
        }
    }
}

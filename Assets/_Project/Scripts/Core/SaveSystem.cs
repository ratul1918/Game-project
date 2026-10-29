using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheLightHouse
{
    [Serializable] public sealed class GameOptionsData
    {
        public bool reducedStorm=true;
        public bool disableBobAndFov;
        public float sensitivity=1f;
        public float volume=1f;
    }
    [Serializable] public sealed class GameSaveData
    {
        public string act="Act1";
        public string checkpointId="start";
        public Vector3 checkpoint;
        public List<string> items=new();
        public List<string> pages=new();
        public List<string> flags=new();
        public GameOptionsData options=new();
    }

    public sealed class SaveSystem : MonoBehaviour
    {
        [SerializeField] string fileName="thelighthouse-save.json";
        GameSaveData data=new();
        HashSet<string> items=new(),pages=new(),flags=new();
        public GameSaveData Data=>data;
        public bool HasSave=>File.Exists(Path.Combine(Application.persistentDataPath,fileName));

        void Awake(){var existing=FindObjectsByType<SaveSystem>();if(existing.Length>1){Destroy(gameObject);return;}DontDestroyOnLoad(gameObject);}
        void OnEnable(){GameEvents.ItemCollected+=Item;GameEvents.PageRead+=Page;GameEvents.StoryBeatTriggered+=Beat;GameEvents.CheckpointReached+=Checkpoint;}
        void OnDisable(){GameEvents.ItemCollected-=Item;GameEvents.PageRead-=Page;GameEvents.StoryBeatTriggered-=Beat;GameEvents.CheckpointReached-=Checkpoint;}
        void Item(ItemCollectedEvent e)=>items.Add(e.Id);
        void Page(PageReadEvent e)=>pages.Add(e.Id);
        void Beat(StoryBeatTriggeredEvent e)=>flags.Add(e.Id);
        void Checkpoint(CheckpointReachedEvent e){data.checkpointId=e.Id;data.checkpoint=e.Position;data.act=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;Save();}
        public void Save()
        {
            data.items=new(items);data.pages=new(pages);data.flags=new(flags);
            File.WriteAllText(Path.Combine(Application.persistentDataPath,fileName),JsonUtility.ToJson(data,true));
        }
        public bool Load()
        {
            string path=Path.Combine(Application.persistentDataPath,fileName);if(!File.Exists(path))return false;
            data=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path))??new GameSaveData();
            items=new(data.items??new());pages=new(data.pages??new());flags=new(data.flags??new());return true;
        }
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
        }
    }
}

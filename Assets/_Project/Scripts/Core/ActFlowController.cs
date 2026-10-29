using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse
{
    public sealed class ActFlowController : MonoBehaviour
    {
        [SerializeField] bool enableSceneChaining;
        [SerializeField] string act1Scene="Act1",act2Scene="Act2",act3Scene="Act3",act4Scene="Act4",act5Scene="Act5";
        SaveSystem saves;
        void Awake(){var existing=FindObjectsByType<ActFlowController>();if(existing.Length>1){Destroy(gameObject);return;}DontDestroyOnLoad(gameObject);saves=GetComponent<SaveSystem>()??gameObject.AddComponent<SaveSystem>();}
        void OnEnable(){GameEvents.StoryBeatTriggered+=Beat;SceneManager.sceneLoaded+=Loaded;}
        void OnDisable(){GameEvents.StoryBeatTriggered-=Beat;SceneManager.sceneLoaded-=Loaded;}
        void Loaded(Scene scene,LoadSceneMode mode)=>saves?.RestoreSceneState();
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(!enableSceneChaining)return;
            string next=e.Id switch {"A1-B06"=>act2Scene,"A2-B07"=>act3Scene,"A3-B09"=>act4Scene,"A4-B06"=>act5Scene,_=>""};
            if(string.IsNullOrEmpty(next))return;saves.Save();SceneManager.LoadScene(next);
        }
        public void StartNew(){saves.NewGame();SceneManager.LoadScene(act1Scene);}
        public void Continue(){if(saves.Load())SceneManager.LoadScene(string.IsNullOrEmpty(saves.Data.act)?act1Scene:saves.Data.act);else StartNew();}
        public void SkipToAct(int act){string[] scenes={act1Scene,act2Scene,act3Scene,act4Scene,act5Scene};SceneManager.LoadScene(scenes[Mathf.Clamp(act-1,0,scenes.Length-1)]);}
        public void ApplyOptions(GameOptionsData options)=>saves?.SetOptions(options);
    }
}

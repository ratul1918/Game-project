using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse
{
    public sealed class ActFlowController : MonoBehaviour
    {
        [SerializeField] bool enableSceneChaining;
        [SerializeField] string act1Scene="Act1",act2Scene="Act2",act3Scene="Act3",act4Scene="Act4",act5Scene="Act5";
        [SerializeField,Min(0f)] float transitionReleaseDelay=.1f;
        SaveSystem saves;
        bool transitioning;
        void Awake(){var existing=FindObjectsByType<ActFlowController>();if(existing.Length>1){Destroy(gameObject);return;}DontDestroyOnLoad(gameObject);saves=GetComponent<SaveSystem>()??gameObject.AddComponent<SaveSystem>();}
        void OnEnable(){GameEvents.StoryBeatTriggered+=Beat;SceneManager.sceneLoaded+=Loaded;}
        void OnDisable(){GameEvents.StoryBeatTriggered-=Beat;SceneManager.sceneLoaded-=Loaded;}
        void Loaded(Scene scene,LoadSceneMode mode){if(mode==LoadSceneMode.Single)saves?.RestoreSceneState();}
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(!enableSceneChaining)return;
            string next=e.Id switch {"A1-B06"=>act2Scene,"A2-B07"=>act3Scene,"A3-B09"=>act4Scene,"A4-B06"=>act5Scene,_=>""};
            if(string.IsNullOrEmpty(next))return;saves.Save();BeginTransition(next);
        }
        public void StartNew(){saves.NewGame();BeginTransition(act1Scene);}
        public void Continue(){if(saves.Load())BeginTransition(string.IsNullOrEmpty(saves.Data.act)?act1Scene:saves.Data.act);else StartNew();}
        public void SkipToAct(int act){string[] scenes={act1Scene,act2Scene,act3Scene,act4Scene,act5Scene};BeginTransition(scenes[Mathf.Clamp(act-1,0,scenes.Length-1)]);}
        public void ApplyOptions(GameOptionsData options)=>saves?.SetOptions(options);
        void BeginTransition(string sceneName){if(!transitioning)StartCoroutine(Transition(sceneName));}
        IEnumerator Transition(string sceneName)
        {
            transitioning=true;Scene previous=SceneManager.GetActiveScene();AsyncOperation load=SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);if(load==null){transitioning=false;yield break;}yield return load;
            Scene next=SceneManager.GetSceneByName(sceneName);if(next.IsValid())SceneManager.SetActiveScene(next);
            if(previous.IsValid()&&previous.isLoaded&&previous!=next)yield return SceneManager.UnloadSceneAsync(previous);
            if(transitionReleaseDelay>0f)yield return new WaitForSecondsRealtime(transitionReleaseDelay);
            yield return Resources.UnloadUnusedAssets();saves?.RestoreSceneState();transitioning=false;
        }
    }
}

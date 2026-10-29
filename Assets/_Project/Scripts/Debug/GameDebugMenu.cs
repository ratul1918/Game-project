using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheLightHouse
{
    public sealed class GameDebugMenu : MonoBehaviour
    {
        [SerializeField] bool allowInRelease;
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] ActFlowController flow;
        [SerializeField] LampController lamp;
        [SerializeField] DrownedManager drowned;
        [SerializeField] StalkerAI stalker;
        [SerializeField] LogbookJournal journal;
        [SerializeField] Transform player;
        [SerializeField] Transform[] teleportPoints;
        [SerializeField,Range(0f,1f)] float debugCharge=.5f;
        [SerializeField] Rect windowRect=new(20,20,310,520);
        [SerializeField] int windowId=7315;
        InputAction toggle;bool shown,figuresFrozen;int teleportIndex;
        void Awake(){toggle=inputActions?.FindActionMap("Player")?.FindAction("GameDebug");}
        void OnEnable(){toggle?.Enable();SceneManager.sceneLoaded+=Loaded;Refresh();}
        void OnDisable(){toggle?.Disable();SceneManager.sceneLoaded-=Loaded;}
        void Loaded(Scene scene,LoadSceneMode mode)=>Refresh();
        void Refresh(){lamp=FindAnyObjectByType<LampController>();drowned=FindAnyObjectByType<DrownedManager>();stalker=FindAnyObjectByType<StalkerAI>();journal=FindAnyObjectByType<LogbookJournal>();var controller=FindAnyObjectByType<FirstPersonController>();if(controller)player=controller.transform;}
        void Update(){if(!Debug.isDebugBuild&&!Application.isEditor&&!allowInRelease)return;if(toggle?.WasPressedThisFrame()??false){shown=!shown;Cursor.visible=shown;Cursor.lockState=shown?CursorLockMode.None:CursorLockMode.Locked;}}
        void OnGUI()
        {
            if(!shown)return;windowRect=GUI.Window(windowId,windowRect,Draw,"The LightHouse Debug");
        }
        void Draw(int id)
        {
            GUILayout.Label("Skip to act");GUILayout.BeginHorizontal();for(int i=1;i<=5;i++){int act=i;if(GUILayout.Button(i.ToString()))flow?.SkipToAct(act);}GUILayout.EndHorizontal();
            if(GUILayout.Button($"Set lamp charge {debugCharge:0.00}"))lamp?.SetCharge(debugCharge);
            if(GUILayout.Button("Give all pages"))journal?.GiveAllPages();
            if(GUILayout.Button("Teleport next")&&player!=null&&teleportPoints!=null&&teleportPoints.Length>0){var target=teleportPoints[teleportIndex++%teleportPoints.Length];if(target)player.position=target.position;}
            if(GUILayout.Button("Freeze/unfreeze figures")){figuresFrozen=!figuresFrozen;drowned?.SetFrozen(figuresFrozen);}
            if(GUILayout.Button("Toggle stalker hearing")&&stalker)stalker.HearingEnabled=!stalker.HearingEnabled;
            if(GUILayout.Button("Trigger DawnReached"))GameEvents.RaiseDawnReached(new(0));
            GUI.DragWindow();
        }
    }
}

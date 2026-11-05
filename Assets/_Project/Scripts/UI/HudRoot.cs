using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class HudRoot : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] UITheme theme;
        [SerializeField] HudSettings settings;
        [SerializeField] RectTransform scalableRoot;

        [Header("HUD")]
        [SerializeField] Image[] noiseSegments;
        [SerializeField] Image staminaFill;
        [SerializeField] Image lampFill;
        [SerializeField] TMP_Text lampState;
        [SerializeField] TMP_Text objectiveText;
        [SerializeField] TMP_Text inventoryText;
        [SerializeField] GameObject noiseRoot;
        [SerializeField] GameObject staminaRoot;
        [SerializeField] GameObject lampRoot;
        [SerializeField] GameObject objectiveRoot;
        [SerializeField] GameObject inventoryRoot;
        [SerializeField] CanvasGroup inventoryGroup;
        [SerializeField] GameObject interactionRoot;
        [SerializeField] CanvasGroup noiseGroup;
        [SerializeField] CanvasGroup staminaGroup;
        [SerializeField] CanvasGroup lampGroup;
        [SerializeField] CanvasGroup objectiveGroup;
        [SerializeField,Min(.1f)] float noiseIdleSeconds=2f;
        [SerializeField,Min(.1f)] float gaugeIdleSeconds=2f;
        [SerializeField,Min(.1f)] float objectiveVisibleSeconds=4f;
        [SerializeField,Min(.05f)] float fadeSeconds=.35f;
        [SerializeField] Color inactiveNoiseColor=new(.16f,.22f,.28f,1f);
        [SerializeField] Color activeNoiseColor=new(.38f,.68f,.82f,1f);
        [SerializeField] Color staminaColor=new(.42f,.54f,.62f,1f);
        [SerializeField] Color lampColor=new(.88f,.58f,.22f,1f);

        [Header("Toasts")]
        [SerializeField] CanvasGroup checkpointToast;
        [SerializeField] TMP_Text checkpointText;
        [SerializeField] CanvasGroup pageToast;
        [SerializeField] TMP_Text pageText;
        [SerializeField] CanvasGroup roomToast;
        [SerializeField] TMP_Text roomText;
        [SerializeField,Min(.1f)] float toastDuration=2f;

        [Header("Composure")]
        [SerializeField] GameObject composureRoot;
        [SerializeField] Image composureFill;
        [SerializeField] Image screenEdgePulse;
        [SerializeField] AudioSource heartbeat;
        [SerializeField,Min(.1f)] float figureRadius=14f;
        [SerializeField,Min(.1f)] float stalkerRadius=18f;
        [SerializeField,Range(0f,1f)] float figureWeight=.4f;
        [SerializeField,Range(0f,1f)] float stalkerWeight=.4f;
        [SerializeField,Range(0f,1f)] float weakLampWeight=.2f;

        Transform player;
        StalkerAI stalker;
        LampState currentLamp;
        Coroutine checkpointRoutine,pageRoutine,roomRoutine,noiseRoutine,staminaRoutine,lampRoutine,objectiveRoutine;
        readonly StringBuilder inventoryBuilder=new();

        void Awake()
        {
            player=FindAnyObjectByType<FirstPersonController>()?.transform;
            stalker=FindAnyObjectByType<StalkerAI>();
            ApplySettings();
            SetAlpha(checkpointToast,0);SetAlpha(pageToast,0);SetAlpha(roomToast,0);
            SetAlpha(noiseGroup,0);SetAlpha(staminaGroup,0);SetAlpha(lampGroup,0);SetAlpha(objectiveGroup,0);
        }

        void OnEnable()
        {
            GameEvents.NoiseEmitted+=Noise;
            GameEvents.StaminaChanged+=Stamina;
            GameEvents.LampStateChanged+=Lamp;
            GameEvents.LampChargeChanged+=Charge;
            GameEvents.ObjectiveChanged+=Objective;
            GameEvents.ItemCollected+=Item;
            GameEvents.CheckpointReached+=Checkpoint;
            GameEvents.PageRead+=Page;
            GameEvents.RoomEntered+=Room;
        }

        void OnDisable()
        {
            GameEvents.NoiseEmitted-=Noise;
            GameEvents.StaminaChanged-=Stamina;
            GameEvents.LampStateChanged-=Lamp;
            GameEvents.LampChargeChanged-=Charge;
            GameEvents.ObjectiveChanged-=Objective;
            GameEvents.ItemCollected-=Item;
            GameEvents.CheckpointReached-=Checkpoint;
            GameEvents.PageRead-=Page;
            GameEvents.RoomEntered-=Room;
        }

        void Update()
        {
            UpdateObjectiveVisibility();
            if(!settings||!settings.ShowComposure||!composureFill)return;
            player??=FindAnyObjectByType<FirstPersonController>()?.transform;
            stalker??=FindAnyObjectByType<StalkerAI>();
            float figures=0f;
            if(player)foreach(var figure in FindObjectsByType<DrownedFigure>(FindObjectsSortMode.None))figures=Mathf.Max(figures,1f-Mathf.Clamp01(Vector3.Distance(player.position,figure.transform.position)/figureRadius));
            float crane=player&&stalker?1f-Mathf.Clamp01(Vector3.Distance(player.position,stalker.transform.position)/stalkerRadius):0f;
            float weak=currentLamp is LampState.Failing or LampState.Extinguished?1f:0f;
            float value=Mathf.Clamp01(figures*figureWeight+crane*stalkerWeight+weak*weakLampWeight);
            composureFill.fillAmount=1f-value;
            if(screenEdgePulse){var color=screenEdgePulse.color;color.a=value*(.25f+.15f*Mathf.Sin(Time.unscaledTime*5f));screenEdgePulse.color=color;}
            if(heartbeat){heartbeat.volume=value;heartbeat.pitch=Mathf.Lerp(.85f,1.3f,value);if(value>.15f&&!heartbeat.isPlaying)heartbeat.Play();else if(value<=.15f&&heartbeat.isPlaying)heartbeat.Stop();}
        }

        public void ApplySettings()
        {
            if(!settings)return;
            if(scalableRoot)scalableRoot.localScale=Vector3.one*settings.HudScale;
            SetActive(noiseRoot,settings.ShowNoise);SetActive(staminaRoot,settings.ShowStamina);SetActive(lampRoot,settings.ShowLamp);SetActive(objectiveRoot,settings.ShowObjective);SetActive(inventoryRoot,settings.ShowInventory);SetActive(interactionRoot,settings.ShowInteraction);SetActive(composureRoot,settings.ShowComposure);
        }

        void Noise(NoiseEmittedEvent value)
        {
            if(!player||!value.Source||value.Source.transform.root!=player.root)return;
            int level=value.Loudness<5f?1:value.Loudness<12f?2:3;
            for(int i=0;i<noiseSegments.Length;i++)if(noiseSegments[i])noiseSegments[i].color=i<level?activeNoiseColor:inactiveNoiseColor;
            ShowGroup(noiseGroup);RestartFade(ref noiseRoutine,noiseGroup,noiseIdleSeconds);
        }
        void Stamina(StaminaChangedEvent value){if(staminaFill)staminaFill.fillAmount=value.Capacity>0?value.Current/value.Capacity:0;ShowGroup(staminaGroup);RestartFade(ref staminaRoutine,staminaGroup,value.Capacity>0&&value.Current>=value.Capacity-.01f?0f:gaugeIdleSeconds);}
        void Lamp(LampStateChangedEvent value){currentLamp=value.Current;SetLamp(value.IntensityNormalized,value.Current);}
        void Charge(float value)=>SetLamp(value,currentLamp);
        void SetLamp(float value,LampState state){if(lampFill){lampFill.fillAmount=Mathf.Clamp01(value);lampFill.color=state==LampState.Failing?theme.Warning:state==LampState.Extinguished?theme.Danger:lampColor;}if(lampState)lampState.text=state.ToString();ShowGroup(lampGroup);RestartFade(ref lampRoutine,lampGroup,value>=.999f?0f:gaugeIdleSeconds);}
        void Objective(string value)
        {
            if (objectiveText)
            {
                objectiveText.text = value != null ? value.Trim() : "";
                objectiveText.textWrappingMode = TextWrappingModes.NoWrap;
                objectiveText.overflowMode = TextOverflowModes.Ellipsis;
            }
            ShowGroup(objectiveGroup);
            RestartFade(ref objectiveRoutine, objectiveGroup, objectiveVisibleSeconds);
        }
        void UpdateObjectiveVisibility(){if(objectiveGroup&&Keyboard.current!=null&&Keyboard.current.anyKey.wasPressedThisFrame){ShowGroup(objectiveGroup);RestartFade(ref objectiveRoutine,objectiveGroup,objectiveVisibleSeconds);}}
        void Item(ItemCollectedEvent value){if(!inventoryText)return;if(inventoryBuilder.Length>0)inventoryBuilder.Append("  •  ");inventoryBuilder.Append(value.Id);inventoryText.text=inventoryBuilder.ToString();ShowGroup(inventoryGroup);}
        void Checkpoint(CheckpointReachedEvent value){ShowToast(ref checkpointRoutine,checkpointToast,checkpointText,$"Checkpoint — {value.Id}");}
        void Page(PageReadEvent value){ShowToast(ref pageRoutine,pageToast,pageText,$"Page added — {value.Id}");}
        void Room(RoomEnteredEvent value){if(settings&&settings.ShowRoomNames)ShowToast(ref roomRoutine,roomToast,roomText,$"{value.DisplayName}\n<size=70%>Floor {value.FloorLevel}</size>",false);}
        void ShowToast(ref Coroutine routine,CanvasGroup group,TMP_Text label,string value,bool respectToastSetting=true){if(!settings||(respectToastSetting&&!settings.ShowToasts)||!group)return;if(routine!=null)StopCoroutine(routine);routine=StartCoroutine(FadeToast(group,label,value));}
        IEnumerator FadeToast(CanvasGroup group,TMP_Text label,string value){if(label)label.text=value;RectTransform rect=group.transform as RectTransform;Vector2 end=rect?rect.anchoredPosition:Vector2.zero;if(rect)rect.anchoredPosition=end+Vector2.up*12f;SetAlpha(group,0);float start=Time.unscaledTime;while(Time.unscaledTime-start<.18f){float t=(Time.unscaledTime-start)/.18f;SetAlpha(group,t);if(rect)rect.anchoredPosition=Vector2.Lerp(end+Vector2.up*12f,end,t);yield return null;}yield return new WaitForSecondsRealtime(toastDuration);start=Time.unscaledTime;while(Time.unscaledTime-start<.35f){SetAlpha(group,1f-(Time.unscaledTime-start)/.35f);yield return null;}SetAlpha(group,0);}
        void ShowGroup(CanvasGroup group){if(group)group.alpha=1f;}
        void RestartFade(ref Coroutine routine,CanvasGroup group,float delay){if(routine!=null)StopCoroutine(routine);routine=delay<=0f?StartCoroutine(FadeGroup(group,0f)):StartCoroutine(FadeGroup(group,delay));}
        IEnumerator FadeGroup(CanvasGroup group,float delay){if(!group)yield break;if(delay>0f)yield return new WaitForSecondsRealtime(delay);float start=Time.unscaledTime;while(Time.unscaledTime-start<fadeSeconds){group.alpha=1f-(Time.unscaledTime-start)/fadeSeconds;yield return null;}group.alpha=0f;}
        static void SetAlpha(CanvasGroup group,float value){if(group)group.alpha=value;}
        static void SetActive(GameObject target,bool value){if(target)target.SetActive(value);}
    }
}

using UnityEngine;
namespace TheLightHouse {
public sealed class DuskTimer : MonoBehaviour {
[SerializeField,Min(1)] float duskSeconds=300;
[SerializeField] Light sun;
[SerializeField] Color duskAmbient = new(0.10f, 0.13f, 0.17f);
[SerializeField] Color darkAmbient = new(0.025f, 0.035f, 0.055f);
[SerializeField] float duskIntensity = 0.45f;
[SerializeField] float darkIntensity = 0.12f;
float elapsed; bool active; bool lit; bool expired;
public bool IsActive=>active&&!expired;
public float Progress=>Mathf.Clamp01(elapsed/Mathf.Max(1f,duskSeconds));
void OnEnable(){GameEvents.GameStateChanged+=State;GameEvents.LampStateChanged+=Lamp;}
void OnDisable(){GameEvents.GameStateChanged-=State;GameEvents.LampStateChanged-=Lamp;}
void State(GameStateChangedEvent e)=>active=e.Current==GameState.Dusk;
void Lamp(LampStateChangedEvent e)=>lit=e.IsLit;
void Update(){if(!active||expired)return; elapsed+=Time.deltaTime; float t=Mathf.Clamp01(elapsed/duskSeconds);if(sun)sun.intensity=Mathf.Lerp(duskIntensity,darkIntensity,t);RenderSettings.ambientLight=Color.Lerp(duskAmbient,darkAmbient,t);
if(t>=1){expired=true;if(!lit){GameEvents.RaiseLampNotLitInTime();Debug.LogWarning("Lamp not lit before dark.");GameEvents.RequestDialogue(new("Daniel","I need to get the lamp lit."));}}}
}}

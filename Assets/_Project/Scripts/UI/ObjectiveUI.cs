using UnityEngine;
using UnityEngine.UI;
namespace TheLightHouse {
public sealed class ObjectiveUI : MonoBehaviour {
[SerializeField] Text label;
CanvasGroup group;
void Awake(){group=GetComponent<CanvasGroup>()??gameObject.AddComponent<CanvasGroup>();Hide();}
void OnEnable()=>GameEvents.ObjectiveChanged+=Show;
void OnDisable()=>GameEvents.ObjectiveChanged-=Show;
void Show(string text){if(label)label.text=text;bool visible=!string.IsNullOrWhiteSpace(text);if(group){group.alpha=visible?1:0;group.blocksRaycasts=visible;}}
void Hide(){if(label)label.text="";if(group){group.alpha=0;group.blocksRaycasts=false;}}
}}

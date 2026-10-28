using UnityEngine;
using UnityEngine.UI;
namespace TheLightHouse {
public sealed class ObjectiveUI : MonoBehaviour {
[SerializeField] Text label;
void OnEnable()=>GameEvents.ObjectiveChanged+=Show;
void OnDisable()=>GameEvents.ObjectiveChanged-=Show;
void Show(string text)=>label.text=text;
}}

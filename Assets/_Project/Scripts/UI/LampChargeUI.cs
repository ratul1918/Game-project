using UnityEngine;
namespace TheLightHouse
{
 public sealed class LampChargeUI:MonoBehaviour
 {
  [SerializeField] Transform gauge;
  [SerializeField] bool screenBar;
  [SerializeField] Rect area=new(20,45,220,25);
  float charge;
  void OnEnable()=>GameEvents.LampChargeChanged+=Changed;
  void OnDisable()=>GameEvents.LampChargeChanged-=Changed;
  void Changed(float value){charge=value;if(gauge){var scale=gauge.localScale;scale.y=Mathf.Max(.001f,value);gauge.localScale=scale;}}
  void OnGUI(){if(screenBar)GUI.Box(area,$"Lamp charge {charge:P0}");}
 }
}

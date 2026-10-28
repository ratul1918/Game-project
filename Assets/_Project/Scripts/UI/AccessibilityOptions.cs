using UnityEngine;
using UnityEngine.InputSystem;
namespace TheLightHouse
{
 public sealed class AccessibilityOptions:MonoBehaviour
 {
  [SerializeField] bool reduceStormVisuals=true;
  [SerializeField] InputActionAsset inputActions;
  InputAction toggle;
  [SerializeField] Rect area=new(20,650,300,30);
  public bool Reduced=>reduceStormVisuals;
  void Awake(){if(inputActions)toggle=inputActions.FindAction("Player/StormAccessibility");}
  void OnEnable()=>toggle?.Enable();
  void OnDisable()=>toggle?.Disable();
  void Update(){if(toggle!=null&&toggle.WasPressedThisFrame())reduceStormVisuals=!reduceStormVisuals;}
  void OnGUI(){reduceStormVisuals=GUI.Toggle(area,reduceStormVisuals,"F8: Reduce lightning and rain");}
 }
}

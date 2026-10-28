using UnityEngine;
namespace TheLightHouse
{
 public sealed class NoiseMasking:MonoBehaviour
 {
  [SerializeField] StormSettings settings;
  float until;
  public float Reduction=>Time.time<until?settings.maskReduction:1;
  void OnEnable()=>GameEvents.Thunder+=Thunder;
  void OnDisable()=>GameEvents.Thunder-=Thunder;
  void Thunder()=>until=Time.time+settings.maskDuration;
 }
}

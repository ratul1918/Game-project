using UnityEngine;
namespace TheLightHouse
{
 [CreateAssetMenu(menuName="TheLightHouse/Storm Settings")]
 public sealed class StormSettings:ScriptableObject
 {
  public float rampDuration=30, rainRate=140, windStrength=1;
  public Vector2 lightningInterval=new(12,22);
  public float flashDuration=.15f, flashIntensity=3, safeFlashIntensity=.25f, thunderDelay=1.5f;
  public float maskDuration=3, maskReduction=.25f;
  public float generatorDelay=5, generatorNoise=24, restartNoise=10;
  public int generatorFlickers=3;
  public float flickerInterval=.15f;
  [Range(0,1)] public float initialCharge=.65f;
  public float chargeGain=.16f, chargeDecay=.025f, healthyThreshold=.35f, extinguishedThreshold=.02f;
  public float crankAcceleration=.45f, crankDeceleration=.25f, noiseInterval=1, crankMaxNoise=20, handleDegreesPerSecond=300;
  public AnimationCurve crankNoiseCurve=AnimationCurve.Linear(0,.1f,1,1);
  public float staminaDrain=6, exhaustionRecovery=3, exhaustedSpeedCap=.25f;
  public float beamDegreesPerSecond=90;
  public float pulseThreshold=.85f,pulseHold=3,pulseCooldown=15;
  public float stairPause=3, stairY=1.2f, lanternY=2.7f, lanternSearchTime=8, lanternHidingCheckChance=.4f;
  public float failingAdvanceInterval=5, extinguishedAdvanceInterval=1.5f, stormHealthyTime=90;
  public int courtyardCount=3;
  public float rainReduction=.2f;
 }
}

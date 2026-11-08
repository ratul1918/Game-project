using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse {
public sealed class WetFootprints : MonoBehaviour, IInteractable {
[SerializeField] string prompt="Inspect footprints";
public string Prompt=>prompt;
public bool RequiresHold=>false;
public void Interact(){GameEvents.RaiseStorySignal("footprints");}
public void StopInteract(){}
[SerializeField] int count=8;
[SerializeField] float spacing=.45f;
[SerializeField] float width=.12f;
[SerializeField] float length=.28f;
[SerializeField] float alternatingOffset=.12f;
[SerializeField] Material footprintMaterial;

public void Reveal()
{
    if (transform.childCount > 0) return;
    if (!footprintMaterial)
    {
        footprintMaterial = Resources.Load<Material>("WetFootprintDecal");
#if UNITY_EDITOR
        if (!footprintMaterial)
        {
            footprintMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/WetFootprintDecal.mat");
        }
#endif
    }

    for (int i = 0; i < count; i++)
    {
        var foot = GameObject.CreatePrimitive(PrimitiveType.Quad);
        foot.name = $"WetFootprint_{i + 1}";
        foot.transform.SetParent(transform, false);
        foot.transform.localPosition = new Vector3(i % 2 == 0 ? alternatingOffset : -alternatingOffset, 0.02f, i * spacing);
        foot.transform.localRotation = Quaternion.Euler(90f, (i % 2 == 0 ? 5f : -8f), 0f);
        foot.transform.localScale = new Vector3(width, length, 1f);
        var r = foot.GetComponent<Renderer>();
        if (r && footprintMaterial)
        {
            r.sharedMaterial = footprintMaterial;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = true;
        }
        var c = foot.GetComponent<Collider>();
        if (c) Destroy(c);
    }
}
}}

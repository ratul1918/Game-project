using UnityEngine;
namespace TheLightHouse
{
    public sealed class CraneHerding : MonoBehaviour
    {
        [SerializeField] Transform boatShed;
        [SerializeField,Range(0,1)] float biasStrength=.08f;
        public Vector3 Bias(Vector3 destination)
        {
            // Intentional foreshadowing: Crane subtly favours the boat shed, protecting his son (Act 4).
            return boatShed?Vector3.Lerp(destination,boatShed.position,biasStrength):destination;
        }
    }
}

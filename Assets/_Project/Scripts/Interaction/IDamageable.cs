using UnityEngine;

namespace TheLightHouse
{
    public interface IDamageable
    {
        void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection, bool isHeavy);
    }
}

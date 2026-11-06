using UnityEngine;

namespace TheLightHouse
{
    public sealed class VolumetricSweepLight : MonoBehaviour
    {
        [SerializeField, Range(1f, 60f)] private float sweepSpeed = 18f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        private void Update()
        {
            transform.Rotate(rotationAxis, sweepSpeed * Time.deltaTime, Space.World);
        }
    }
}

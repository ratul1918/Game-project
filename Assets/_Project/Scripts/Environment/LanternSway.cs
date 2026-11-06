using UnityEngine;

namespace TheLightHouse
{
    public sealed class LanternSway : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 10f)] private float swaySpeed = 1.2f;
        [SerializeField, Range(0.5f, 20f)] private float swayAngle = 3.5f;
        [SerializeField] private Vector3 swayAxis = Vector3.forward;

        private float randomOffset;
        private Quaternion initialRotation;

        private void Start()
        {
            initialRotation = transform.localRotation;
            randomOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            float angle = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayAngle;
            transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, swayAxis);
        }
    }
}

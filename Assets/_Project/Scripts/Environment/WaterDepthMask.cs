using UnityEngine;

namespace TheLightHouse
{
    /// <summary>
    /// Water exclusion volume component covering subterranean spaces (cellar stairs, vault, radio room)
    /// to cull and exclude ocean water penetration underground.
    /// </summary>
    [ExecuteAlways]
    public sealed class WaterDepthMask : MonoBehaviour
    {
        [Header("Exclusion Bounds")]
        [SerializeField] private Vector3 volumeCenter = new(-8.0f, -2.5f, 3.5f);
        [SerializeField] private Vector3 volumeSize = new(9.0f, 6.0f, 18.0f);
        [SerializeField] private bool activeMask = true;

        public Bounds ExclusionBounds => new(transform.TransformPoint(volumeCenter), volumeSize);

        private void OnEnable()
        {
            ApplyMask();
        }

        private void OnDisable()
        {
            RemoveMask();
        }

        private void Update()
        {
            if (activeMask)
            {
                ApplyMask();
            }
        }

        public void ApplyMask()
        {
            Vector3 worldCenter = transform.TransformPoint(volumeCenter);
            Shader.SetGlobalVector("_CellarWaterMaskCenter", worldCenter);
            Shader.SetGlobalVector("_CellarWaterMaskSize", volumeSize);
            Shader.SetGlobalFloat("_CellarWaterMaskActive", activeMask ? 1.0f : 0.0f);
        }

        public void RemoveMask()
        {
            Shader.SetGlobalFloat("_CellarWaterMaskActive", 0.0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1.0f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(volumeCenter, volumeSize);
            Gizmos.color = new Color(0.2f, 0.7f, 1.0f, 0.9f);
            Gizmos.DrawWireCube(volumeCenter, volumeSize);
        }
    }
}

using UnityEngine;

namespace TheLightHouse
{
    /// <summary>
    /// Invisible trigger box safety plane placed below the island.
    /// Resets player position back to the dock entrance if they fall through any collider void.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class FallbackSafetyPlane : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private Vector3 fallbackPosition = new(-1.1f, 0.22f, -33.5f);

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (respawnPoint == null)
            {
                var dockObj = GameObject.Find("Dock Jetty") ?? GameObject.Find("Dock");
                if (dockObj != null) respawnPoint = dockObj.transform;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var fpc = other.GetComponent<FirstPersonController>() ?? other.GetComponentInParent<FirstPersonController>();
            var cc = other.GetComponent<CharacterController>() ?? other.GetComponentInParent<CharacterController>();

            if (fpc == null && cc == null && !other.CompareTag("Player"))
            {
                return;
            }

            Transform targetTransform = fpc != null ? fpc.transform : (cc != null ? cc.transform : other.transform);
            Vector3 pos = targetTransform.position;

            // Legitimate subterranean cellar bounds: x in [-14.5, -5.5], z in [-5.5, 13.0], y down to -5.0m
            bool inCellar = pos.x >= -14.5f && pos.x <= -5.5f && pos.z >= -5.5f && pos.z <= 13.0f;
            if (inCellar && pos.y >= -5.5f)
            {
                // Player is traversing the legitimate underground cellar stairs or secret radio room
                return;
            }

            Vector3 respawn = respawnPoint != null ? respawnPoint.position : fallbackPosition;
            respawn.y = Mathf.Max(respawn.y, 0.22f);

            if (cc != null) cc.enabled = false;
            targetTransform.position = respawn;
            if (cc != null) cc.enabled = true;

            Debug.LogWarning($"[FallbackSafetyPlane] Void fall intercepted at {pos}. Reset player to dock entrance: {respawn}.");
        }
    }
}

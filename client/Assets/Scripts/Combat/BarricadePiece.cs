using UnityEngine;
using UnityEngine.AI;
using EmberBlade.AI;
using EmberBlade.Config;

namespace EmberBlade.Combat
{
    public enum BarricadePieceType
    {
        Wall,
        SpikeTrap
    }

    public class BarricadePiece : MonoBehaviour
    {
        [SerializeField] private BarricadePieceType pieceType;
        [SerializeField] private GameConfig config;
        private float nextDamageTime;

        public BarricadePieceType PieceType => pieceType;

        public void Initialize(BarricadePieceType type, GameConfig gameConfig)
        {
            pieceType = type;
            config = gameConfig;
            if (pieceType == BarricadePieceType.Wall)
            {
                NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();
                if (obstacle == null)
                {
                    obstacle = gameObject.AddComponent<NavMeshObstacle>();
                }
                obstacle.carving = true;
                obstacle.size = new Vector3(config.barricadeWallWidth, config.barricadeWallHeight, 0.35f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (pieceType != BarricadePieceType.SpikeTrap || Time.time < nextDamageTime)
            {
                return;
            }

            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            if (enemy == null)
            {
                return;
            }

            nextDamageTime = Time.time + config.barricadeTrapCooldown;
            enemy.TakeDamage(config.barricadeTrapDamage);
        }
    }
}

using UnityEngine;

namespace EmberBlade.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Ember Blade/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player")]
        public float playerMoveSpeed = 5f;
        public float playerRotationSpeed = 12f;
        public float cameraDistance = 6f;
        public float cameraHeight = 2.5f;
        public float cameraLookSensitivity = 0.12f;

        [Header("Combat")]
        public int playerMaxHealth = 100;
        public int lightAttackDamage = 25;
        public float lightAttackCooldown = 0.45f;
        public float lightAttackRange = 1.6f;
        public float lightAttackRadius = 0.65f;
        public float dodgeDuration = 0.35f;
        public float dodgeCooldown = 1.2f;
        public float dodgeDistance = 3f;
        public float parryWindow = 0.25f;
        public float parryCooldown = 0.6f;
        public int parryEmberGain = 20;
        public int emberMax = 100;
        public float emberEmpoweredDamageMultiplier = 2f;

        [Header("Enemies")]
        public int scoutHealth = 45;
        public float scoutSpeed = 4f;
        public int scoutDamage = 8;
        public float scoutAttackRange = 1.35f;
        public float scoutAttackCooldown = 1.4f;
        public int skirmisherHealth = 80;
        public float skirmisherSpeed = 2.5f;
        public int skirmisherDamage = 14;
        public float skirmisherAttackRange = 1.5f;
        public float skirmisherAttackCooldown = 2f;
        public float skirmisherWindup = 0.4f;
        public float enemyChaseRange = 18f;

        [Header("Test Wave")]
        public int scoutCount = 3;
        public int skirmisherCount = 2;
    }
}

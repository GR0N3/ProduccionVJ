using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyType", menuName = "Enemies/Enemy Type")]
    public class EnemyType : ScriptableObject
    {
        [Header("Basic Stats")]
        public string EnemyName = "Enemy";
        public float MaxHealth = 100f;

        [Header("Movement")]
        public float WalkSpeed = 2f;
        public float ChaseSpeed = 3f;
        public float DetectionRange = 5f;
        public float AttackRange = 1.5f;

        [Header("Combat")]
        public float AttackDamage = 10f;
        public float AttackCooldown = 1f;

        [Header("Sounds")]
        public AudioClip AttackSound;
        public AudioClip HitSound;
        public AudioClip DeathSound;
        public AudioClip WalkSound;
    }
}

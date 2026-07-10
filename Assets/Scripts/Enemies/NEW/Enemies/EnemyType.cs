using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyType", menuName = "Enemies/Enemy Type")]
    public class EnemyType : ScriptableObject
    {
        [Header("Basic Stats")]
        public string EnemyName = "Enemy";
        public int MaxHealth = 1;

        [Header("Movement")]
        public float WalkSpeed = 2f;
        public float ChaseSpeed = 3f;
        public float DetectionRange = 5f;
        public float AttackRange = 1.5f;

        [Header("Combat")]
        public int AttackDamage = 1;
        public float AttackCooldown = 1f;

        [Header("Drops")]
        public GameObject[] Drops;

        [Header("Sounds")]
        public AudioClip AttackSound;
        public AudioClip HitSound;
        public AudioClip DeathSound;
        public AudioClip WalkSound;
    }
}

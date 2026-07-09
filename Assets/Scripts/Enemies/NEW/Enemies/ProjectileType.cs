using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(fileName = "NewProjectileType", menuName = "Enemies/Projectile Type")]
    public class ProjectileType : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Velocidad de desplazamiento del proyectil")]
        public float Speed = 10f;

        [Tooltip("Segundos antes de que el proyectil se desactive automáticamente")]
        public float LifeTime = 3f;

        [Header("Combat")]
        [Tooltip("Daño que inflige al impactar")]
        public float Damage = 15f;

        [Header("Sounds")]
        public AudioClip LaunchSound;
        public AudioClip ImpactSound;
    }
}

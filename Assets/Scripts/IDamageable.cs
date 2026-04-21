using UnityEngine;

public interface IDamageable
{
    bool TakeDamage(int damage, Vector2 hitDirection, float knockbackForce);
}
using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int maxHealth = 3;
    public float knockbackResistance = 1f;

    public static event Action OnEnemyDeath;

    public int currentHealth;
    private Rigidbody2D rb;

    public bool IsDead => currentHealth <= 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void ApplyKnockback(Vector2 direction, float force)
    {
        Vector2 finalForce = direction.normalized * force / knockbackResistance;

        rb.AddForce(finalForce, ForceMode2D.Impulse);
    }

    void Die()
    {
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }

    public bool TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (IsDead) return false;

        currentHealth -= damage;

        ApplyKnockback(hitDirection, knockbackForce);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }
}
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int maxHealth = 3;
    public float knockbackResistance = 1f;

    private int currentHealth;
    private Rigidbody2D rb;

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
        // Acá podés meter animaciones, partículas, etc
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        currentHealth -= damage;

        ApplyKnockback(hitDirection, knockbackForce);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}
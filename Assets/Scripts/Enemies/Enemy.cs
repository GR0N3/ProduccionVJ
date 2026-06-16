using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Salud")]
    public int maxHealth = 3;
    public float knockbackResistance = 1f;

    [Header("Ataque al Jugador")]
    public int damageToPlayer = 1;
    public float knockbackToPlayer = 10f;
    [Tooltip("Segundos que se queda quieto tras RECIBIR daño")]
    public float stunDuration = 0.5f;

    // Estados de control externos
    public bool isStunned = false;
    public bool isBlocking = false;

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

        // Si el escudo está levantado, se anula la entrada de daño y empuje.
        if (isBlocking)
        {
            return false;
        }

        currentHealth -= damage;
        ApplyKnockback(hitDirection, knockbackForce);

        StartCoroutine(StunRoutine(stunDuration));

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private IEnumerator StunRoutine(float time)
    {
        isStunned = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(time);

        isStunned = false;
    }
}
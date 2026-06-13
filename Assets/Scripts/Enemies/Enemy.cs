using System;
using System.Collections; // Necesario para los tiempos de espera
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Salud")]
    public int maxHealth = 3;
    public float knockbackResistance = 1f;

    // --- NUEVO: CONFIGURACIÓN PARA ATACAR AL JUGADOR ---
    [Header("Ataque al Jugador")]
    public int damageToPlayer = 1;
    [Tooltip("Fuerza con la que este enemigo empuja al jugador")]
    public float knockbackToPlayer = 10f;
    [Tooltip("Segundos que se queda quieto tras golpear")]
    public float stunDuration = 2f;

    // Esta variable le avisará a otros scripts si el enemigo está atontado
    public bool isStunned = false;

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

        // EXTRA: Si VOS le pegás al enemigo, también se queda atontado un ratito
        StartCoroutine(StunRoutine(stunDuration / 2f));

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    // --- NUEVO: LA MAGIA PARA GOLPEAR AL JUGADOR ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GolpearJugador(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GolpearJugador(collision.gameObject);
    }

    private void GolpearJugador(GameObject objetoTocado)
    {
        PlayerController player = objetoTocado.GetComponent<PlayerController>();

        // Si tocamos al jugador, y no estamos ni muertos ni ya atontados...
        if (player != null && !IsDead && !isStunned)
        {
            // 1. Calculamos hacia dónde empujar al jugador
            Vector2 direccionEmpuje = (player.transform.position - transform.position).normalized;
            direccionEmpuje.y = 0.5f; // Lo levanta un poco del piso

            // 2. Le mandamos el daño y TU fuerza de empuje personalizada
            player.TakeDamage(damageToPlayer, direccionEmpuje, knockbackToPlayer);

            // 3. Iniciamos la pausa de 2 segundos
            StartCoroutine(StunRoutine(stunDuration));
        }
    }

    // --- SISTEMA DE PAUSA (COOLDOWN) ---
    private IEnumerator StunRoutine(float time)
    {
        isStunned = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Le clavamos los frenos

        yield return new WaitForSeconds(time); // Espera los 2 segundos exactos

        isStunned = false; // Ya puede volver a moverse y atacar
    }
}
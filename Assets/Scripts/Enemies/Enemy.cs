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
    public float stunDuration = 0.5f;

    [Header("Efecto de Muerte")]
    [Tooltip("Tiene que durar lo mismo o un poquito más que tu animación de muerte para que no desaparezca antes de terminar de caer.")]
    public float fadeDuration = 1f;

    public bool isStunned = false;
    public bool isBlocking = false;

    public static event Action OnEnemyDeath;

    public int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // 
    private Color colorOriginal;

    private Coroutine stunCoroutine;
    private Coroutine flashCoroutine;

    public bool IsDead => currentHealth <= 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        currentHealth = maxHealth;
    }

    void Die()
    {
        OnEnemyDeath?.Invoke();
        StartCoroutine(RutinaMuerteDesvanecimiento());
    }

    public bool TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (IsDead) return false;
        if (isBlocking) return false;

        currentHealth -= damage;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        else
        {
            flashCoroutine = StartCoroutine(RutinaParpadeo());
            stunCoroutine = StartCoroutine(StunRoutine(stunDuration, hitDirection, knockbackForce));
        }

        return false;
    }

    private IEnumerator RutinaParpadeo()
    {
        float tiempo = 0f;
        bool colorCambiado = false;

        while (tiempo < 1f)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = colorCambiado ? colorOriginal : new Color(1f, 0.4f, 0.4f, 0.8f);
                colorCambiado = !colorCambiado;
            }

            tiempo += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }

    private IEnumerator StunRoutine(float time, Vector2 direction, float force)
    {
        isStunned = true;

        rb.linearVelocity = Vector2.zero;

        Vector2 finalForce = direction.normalized * force / knockbackResistance;
        rb.AddForce(finalForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(time);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isStunned = false;
    }

    private IEnumerator RutinaMuerteDesvanecimiento()
    {
        Collider2D[] colisionadores = GetComponents<Collider2D>();
        foreach (Collider2D col in colisionadores)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorOriginal;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < fadeDuration)
            {
                tiempoTranscurrido += Time.deltaTime;
                float nuevoAlpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / fadeDuration);

                spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, nuevoAlpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
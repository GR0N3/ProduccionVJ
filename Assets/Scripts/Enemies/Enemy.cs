using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Salud")]
    public int maxHealth = 3;
    public float knockbackResistance = 1f;

    [Header("UI de Vida")]
    [Tooltip("Arrastra aquí el Slider de la barra de vida del enemigo")]
    public Slider healthBar; 

    [Header("Ataque al Jugador")]
    public int damageToPlayer = 1;
    public float knockbackToPlayer = 10f;
    public float stunDuration = 0.5f;

    [Header("Efecto de Muerte")]
    public float fadeDuration = 1f;

    [Header("Drop de Vida")]
    public GameObject healthDropPrefab;
    [Range(0f, 100f)] public float dropChance = 30f;

    [Header("Mecánica Anti Stun-Lock")]
    public int golpesParaInmunidad = 3;
    public float duracionInmunidad = 1.5f;

    private int golpesRecibidosSeguidos = 0;
    private bool esInmuneAlStun = false;
    private float temporizadorInmunidad = 0f;

    public bool isStunned = false;
    public bool isBlocking = false;

    public static event Action OnEnemyDeath;

    public int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
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

        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (esInmuneAlStun)
        {
            temporizadorInmunidad -= Time.deltaTime;
            if (temporizadorInmunidad <= 0f)
            {
                esInmuneAlStun = false;
                golpesRecibidosSeguidos = 0;
            }
        }
    }

    void Die()
    {
        
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        OnEnemyDeath?.Invoke();
        GenerarDrop();
        StartCoroutine(RutinaMuerteDesvanecimiento());
    }

    private void GenerarDrop()
    {
        if (healthDropPrefab != null)
        {
            float probabilidadAleatoria = UnityEngine.Random.Range(0f, 100f);
            if (probabilidadAleatoria <= dropChance)
            {
                Instantiate(healthDropPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    public bool TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (IsDead) return false;

        if (isBlocking)
        {
            bool golpePorLaEspalda = (transform.localScale.x * hitDirection.x) > 0f;

            if (!golpePorLaEspalda)
            {
                return false;
            }
            isBlocking = false;
        }

        currentHealth -= damage;

        
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            Die();
            return true;
        }

        golpesRecibidosSeguidos++;

        if (golpesRecibidosSeguidos >= golpesParaInmunidad && !esInmuneAlStun)
        {
            esInmuneAlStun = true;
            temporizadorInmunidad = duracionInmunidad;

            isStunned = false;
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(RutinaParpadeo());

        if (!esInmuneAlStun)
        {
            if (stunCoroutine != null) StopCoroutine(stunCoroutine);
            stunCoroutine = StartCoroutine(StunRoutine(stunDuration, hitDirection, knockbackForce));
        }

        return false;
    }

    private IEnumerator RutinaParry() { yield return null; }

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
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (rb != null && knockbackResistance > 0)
        {
            Vector2 finalForce = direction.normalized * force / knockbackResistance;
            rb.AddForce(finalForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(time);

        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
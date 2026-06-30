using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 🔥 NUEVO: Necesario para controlar la barra de vida

public class FlyingEnemy : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    public int health = 3;

    // 🔥 NUEVO: Referencia a la barra de vida
    [Header("UI de Vida")]
    [Tooltip("Arrastra aquí el Slider de la barra de vida del enemigo volador")]
    public Slider healthBar;

    [Header("Patrulla (Cielo)")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;

    [Header("Persecución")]
    public float diveSpeed = 6f;
    public float detectionRadius = 6f;
    public float loseInterestRadius = 12f;

    [Header("Ataque (Sin Daño de Contacto)")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public float attackDelay = 0.3f;
    public float knockbackForce = 12f;

    [Header("Enfriamiento (Cooldown)")]
    public float attackCooldown = 2f;
    public float hoverHeight = 2.5f;

    [Header("Efecto de Muerte")]
    public float duracionDesvanecimiento = 1f;

    [Header("Detección de Paredes y Visión")]
    [Tooltip("Asegurate de que esta capa tenga tildado 'Ground' o las paredes, sino tendrá visión de rayos X")]
    public LayerMask capaObstaculos;
    public float distanciaEvasion = 1f;

    // 🔥 --- NUEVO: SISTEMA DE DROP --- 🔥
    [Header("Drop de Vida")]
    [Tooltip("El Prefab del corazón o poción que va a soltar.")]
    public GameObject healthDropPrefab;
    [Tooltip("Probabilidad de que suelte la vida al morir (0 = Nunca, 100 = Siempre)")]
    [Range(0f, 100f)]
    public float dropChance = 30f;
    // ------------------------------------

    private float cooldownTimer;
    private Vector2 startPos;
    private bool movingRight = true;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal = Color.white;
    private Coroutine flashCoroutine;

    private bool isAttacking = false;
    private bool isDead = false;

    private enum BatState { Patrullando, Persiguiendo, Recuperando, Regresando }
    private BatState estadoActual = BatState.Patrullando;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

        if (rb != null) rb.gravityScale = 0f;

        startPos = transform.position;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) playerTransform = player.transform;

        // 🔥 NUEVO: Inicializamos la barra de vida al máximo
        if (healthBar != null)
        {
            healthBar.maxValue = health;
            healthBar.value = health;
        }
    }

    void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;

        if (isAttacking) return;

        if (estadoActual == BatState.Persiguiendo || estadoActual == BatState.Recuperando)
        {
            float distanciaAlJugador = Vector2.Distance(transform.position, playerTransform.position);
            if (distanciaAlJugador > loseInterestRadius)
            {
                estadoActual = BatState.Regresando;
            }
        }

        switch (estadoActual)
        {
            case BatState.Patrullando:
                Patrullar();
                break;
            case BatState.Persiguiendo:
                EjecutarPersecucion();
                break;
            case BatState.Recuperando:
                Recuperarse();
                break;
            case BatState.Regresando:
                RegresarACasa();
                break;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", rb.linearVelocity.magnitude > 0.1f);
        }
    }

    void Patrullar()
    {
        BuscarJugador();

        float limiteDer = startPos.x + patrolDistance;
        float limiteIzq = startPos.x - patrolDistance;

        Vector2 direccionMirada = movingRight ? Vector2.right : Vector2.left;
        if (Physics2D.Raycast(transform.position, direccionMirada, distanciaEvasion, capaObstaculos))
        {
            movingRight = !movingRight;
        }

        if (movingRight)
        {
            rb.linearVelocity = new Vector2(patrolSpeed, 0);
            transform.localScale = new Vector3(1, 1, 1);
            if (transform.position.x >= limiteDer) movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, 0);
            transform.localScale = new Vector3(-1, 1, 1);
            if (transform.position.x <= limiteIzq) movingRight = true;
        }
    }

    void BuscarJugador()
    {
        float distancia = Vector2.Distance(transform.position, playerTransform.position);

        if (distancia <= detectionRadius)
        {
            Vector2 direccionAlJugador = (playerTransform.position - transform.position).normalized;
            RaycastHit2D paredEnElMedio = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, capaObstaculos);

            if (paredEnElMedio.collider == null)
            {
                estadoActual = BatState.Persiguiendo;
            }
        }
    }

    void EjecutarPersecucion()
    {
        float distanciaAlJugador = Vector2.Distance(transform.position, playerTransform.position);

        if (distanciaAlJugador <= attackRange)
        {
            StartCoroutine(RutinaAtaque());
        }
        else
        {
            Vector2 direccion = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direccion * diveSpeed;

            if (playerTransform.position.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
            else transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private IEnumerator RutinaAtaque()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        if (playerTransform != null && !isDead)
        {
            float distanciaAlJugador = Vector2.Distance(transform.position, playerTransform.position);

            if (distanciaAlJugador <= attackRange)
            {
                PlayerController player = playerTransform.GetComponent<PlayerController>();
                if (player != null && !player.isDead)
                {
                    Vector2 direccionEmpuje = (player.transform.position - transform.position).normalized;
                    direccionEmpuje.y = 0.5f;
                    player.TakeDamage(attackDamage, direccionEmpuje, knockbackForce);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        if (!isDead) IniciarRecuperacion();
    }

    void Recuperarse()
    {
        cooldownTimer -= Time.fixedDeltaTime;

        Vector2 hoverPosition = new Vector2(playerTransform.position.x, playerTransform.position.y + hoverHeight);
        Vector2 direccionDeseada = (hoverPosition - (Vector2)transform.position).normalized;

        direccionDeseada = CalcularEvasion(direccionDeseada);

        rb.linearVelocity = direccionDeseada * (diveSpeed * 0.6f);

        if (direccionDeseada.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);

        if (cooldownTimer <= 0) estadoActual = BatState.Persiguiendo;
    }

    void RegresarACasa()
    {
        BuscarJugador();

        Vector2 direccionDeseada = (startPos - (Vector2)transform.position).normalized;
        direccionDeseada = CalcularEvasion(direccionDeseada);

        rb.linearVelocity = direccionDeseada * patrolSpeed;

        if (direccionDeseada.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else transform.localScale = new Vector3(-1, 1, 1);

        if (Vector2.Distance(transform.position, startPos) < 0.5f)
        {
            estadoActual = BatState.Patrullando;
        }
    }

    Vector2 CalcularEvasion(Vector2 direccionDeseada)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionDeseada, distanciaEvasion, capaObstaculos);

        if (hit.collider != null)
        {
            Vector2 normal = hit.normal;
            Vector2 direccionTangente = direccionDeseada - Vector2.Dot(direccionDeseada, normal) * normal;

            if (direccionTangente.magnitude < 0.1f)
            {
                direccionTangente = new Vector2(-normal.y, normal.x);
                if (direccionTangente.y < 0) direccionTangente = -direccionTangente;
            }

            return (direccionTangente.normalized + normal * 0.5f).normalized;
        }

        return direccionDeseada;
    }

    void IniciarRecuperacion()
    {
        estadoActual = BatState.Recuperando;
        cooldownTimer = attackCooldown;
    }

    private void GenerarDrop()
    {
        if (healthDropPrefab != null)
        {
            float probabilidadAleatoria = Random.Range(0f, 100f);
            if (probabilidadAleatoria <= dropChance)
            {
                Instantiate(healthDropPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    public bool TakeDamage(int damage, Vector2 direction, float knockback)
    {
        if (isDead) return false;

        health -= damage;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockback, ForceMode2D.Impulse);

        // 🔥 NUEVO: Actualizamos el valor de la barra de vida al recibir daño
        if (healthBar != null)
        {
            healthBar.value = health;
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        if (health <= 0)
        {
            StartCoroutine(SecuenciaMuerteEnemigo());
        }
        else
        {
            flashCoroutine = StartCoroutine(RutinaParpadeo());
        }

        return true;
    }

    private IEnumerator RutinaParpadeo()
    {
        float tiempo = 0f;
        bool colorCambiado = false;

        while (tiempo < 0.5f)
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

    private IEnumerator SecuenciaMuerteEnemigo()
    {
        isDead = true;

        // 🔥 NUEVO: Ocultamos la barra de vida al morir
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        GenerarDrop();

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D colisionador = GetComponent<Collider2D>();
        if (colisionador != null) colisionador.enabled = false;

        if (animator != null) animator.SetTrigger("Death");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorOriginal;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < duracionDesvanecimiento)
            {
                tiempoTranscurrido += Time.deltaTime;
                float nuevoAlpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / duracionDesvanecimiento);

                spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, nuevoAlpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, loseInterestRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaEvasion);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
using System.Collections;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    public int health = 3;

    [Header("Patrulla (Cielo)")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;

    [Header("Persecución")]
    public float diveSpeed = 6f;
    public float detectionRadius = 6f;
    public float loseInterestRadius = 12f;

    [Header("Ataque (Sin Daño de Contacto)")]
    public int attackDamage = 1;
    [Tooltip("Distancia a la que se frena para intentar morderte")]
    public float attackRange = 1.5f;
    [Tooltip("Fracción de segundo que tarda la animación en dar el golpe (para esquivarlo)")]
    public float attackDelay = 0.3f;
    public float knockbackForce = 12f;

    [Header("Enfriamiento (Cooldown)")]
    public float attackCooldown = 2f;
    public float hoverHeight = 2.5f;

    [Header("Efecto de Muerte")]
    [Tooltip("Tiempo en segundos que tardará el enemigo en desaparecer por completo.")]
    public float duracionDesvanecimiento = 1f;

    [Header("Detección de Paredes")]
    [Tooltip("Capa que el murciélago detectará como pared (Ej: Ground)")]
    public LayerMask capaObstaculos;
    public float distanciaEvasion = 1f;

    private float cooldownTimer;
    private Vector2 startPos;
    private bool movingRight = true;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // Referencia para controlar la opacidad
    private bool isAttacking = false;
    private bool isDead = false; // Bloqueo de seguridad para el estado de muerte

    private enum BatState { Patrullando, Persiguiendo, Recuperando, Regresando }
    private BatState estadoActual = BatState.Patrullando;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Se busca el SpriteRenderer en el objeto o en sus componentes hijos
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb != null) rb.gravityScale = 0f;

        startPos = transform.position;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) playerTransform = player.transform;
    }

    void FixedUpdate()
    {
        // Si el enemigo está muerto o no hay jugador, se interrumpe toda la ejecución física e IA
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
            estadoActual = BatState.Persiguiendo;
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

    public bool TakeDamage(int damage, Vector2 direction, float knockback)
    {
        if (isDead) return false;

        health -= damage;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockback, ForceMode2D.Impulse);

        if (health <= 0)
        {
            StartCoroutine(SecuenciaMuerteEnemigo());
        }
        return true;
    }

    // --- NUEVA CORRUTINA DE DESVANECIMIENTO ---
    private IEnumerator SecuenciaMuerteEnemigo()
    {
        isDead = true;

        // Detiene el movimiento por completo y lo desvincula de las fuerzas físicas aplicadas
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Desactiva el colisionador para que el jugador pase a través del sprite inerte
        Collider2D colisionador = GetComponent<Collider2D>();
        if (colisionador != null) colisionador.enabled = false;

        // Si existe un trigger "Death" en el Animator, se ejecuta aquí
        if (animator != null) animator.SetTrigger("Death");

        if (spriteRenderer != null)
        {
            Color colorInicial = spriteRenderer.color;
            float tiempoTranscurrido = 0f;

            // Bucle de interpolación lineal para reducir el canal alpha
            while (tiempoTranscurrido < duracionDesvanecimiento)
            {
                tiempoTranscurrido += Time.deltaTime;
                float nuevoAlpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / duracionDesvanecimiento);

                spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, nuevoAlpha);
                yield return null; // Espera al siguiente fotograma
            }
        }

        // Una vez alcanzada la opacidad cero, se elimina el objeto de la memoria de la escena
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
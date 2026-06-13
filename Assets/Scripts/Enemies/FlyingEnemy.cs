using UnityEngine;

public class FlyingEnemy : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    public int health = 3;

    [Header("Patrulla (Cielo)")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;

    [Header("Ataque (Picada)")]
    public float diveSpeed = 6f;
    public float detectionRadius = 6f;
    public float loseInterestRadius = 12f;
    public float knockbackForce = 12f;

    [Header("Enfriamiento (Cooldown)")]
    public float attackCooldown = 2f;
    public float hoverHeight = 2.5f;

    [Header("Detección de Paredes")]
    [Tooltip("Capa que el murciélago detectará como pared (Ej: Ground)")]
    public LayerMask capaObstaculos;
    [Tooltip("Largo del sensor láser para detectar y esquivar")]
    public float distanciaEvasion = 1f;

    private float cooldownTimer;
    private Vector2 startPos;
    private bool movingRight = true;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private enum BatState { Patrullando, Atacando, Recuperando, Regresando }
    private BatState estadoActual = BatState.Patrullando;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) playerTransform = player.transform;
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        if (estadoActual == BatState.Atacando || estadoActual == BatState.Recuperando)
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
                BuscarJugador();
                break;
            case BatState.Atacando:
                Abalanzarse();
                break;
            case BatState.Recuperando:
                Recuperarse();
                break;
            case BatState.Regresando:
                RegresarACasa();
                BuscarJugador(); // CORRECCIÓN: Permite detectar al jugador en el trayecto de regreso
                break;
        }
    }

    void Patrullar()
    {
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
            transform.localScale = new Vector3(-1, 1, 1);
            if (transform.position.x >= limiteDer) movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, 0);
            transform.localScale = new Vector3(1, 1, 1);
            if (transform.position.x <= limiteIzq) movingRight = true;
        }
    }

    void BuscarJugador()
    {
        float distancia = Vector2.Distance(transform.position, playerTransform.position);
        if (distancia <= detectionRadius && playerTransform.position.y < transform.position.y)
        {
            estadoActual = BatState.Atacando;
        }
    }

    void Abalanzarse()
    {
        Vector2 direccion = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direccion * diveSpeed;

        if (playerTransform.position.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    void Recuperarse()
    {
        cooldownTimer -= Time.fixedDeltaTime;

        Vector2 hoverPosition = new Vector2(playerTransform.position.x, playerTransform.position.y + hoverHeight);
        Vector2 direccionDeseada = (hoverPosition - (Vector2)transform.position).normalized;

        direccionDeseada = CalcularEvasion(direccionDeseada);

        rb.linearVelocity = direccionDeseada * patrolSpeed;

        if (direccionDeseada.x > 0) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);

        if (cooldownTimer <= 0) estadoActual = BatState.Atacando;
    }

    void RegresarACasa()
    {
        Vector2 direccionDeseada = (startPos - (Vector2)transform.position).normalized;
        direccionDeseada = CalcularEvasion(direccionDeseada);

        rb.linearVelocity = direccionDeseada * patrolSpeed;

        if (direccionDeseada.x > 0) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            Vector2 direccionEmpuje = (player.transform.position - transform.position).normalized;
            direccionEmpuje.y = 0.5f;

            player.TakeDamage(1, direccionEmpuje, knockbackForce);
            IniciarRecuperacion();
        }
        else
        {
            if (estadoActual == BatState.Atacando) IniciarRecuperacion();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (estadoActual == BatState.Atacando && collision.gameObject.GetComponent<PlayerController>() == null)
        {
            IniciarRecuperacion();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector2 direccionEmpuje = (player.transform.position - transform.position).normalized;
            direccionEmpuje.y = 0.5f;

            player.TakeDamage(1, direccionEmpuje, knockbackForce);
            IniciarRecuperacion();
        }
    }

    public bool TakeDamage(int damage, Vector2 direction, float knockback)
    {
        health -= damage;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockback, ForceMode2D.Impulse);

        if (health <= 0) Destroy(gameObject);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, loseInterestRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaEvasion);
    }
}
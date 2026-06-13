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
    public float knockbackForce = 12f;

    // --- NUEVO: SISTEMA DE ENFRIAMIENTO (COOLDOWN) ---
    [Header("Enfriamiento (Cooldown)")]
    [Tooltip("Segundos que espera antes de volver a atacar")]
    public float attackCooldown = 2f;
    [Tooltip("Altura a la que vuela sobre el jugador mientras espera")]
    public float hoverHeight = 2.5f;

    private float cooldownTimer;
    private Vector2 startPos;
    private bool movingRight = true;
    private Transform playerTransform;

    // Cambiamos 'Regresando' por 'Recuperando'
    private enum BatState { Patrullando, Atacando, Recuperando }
    private BatState estadoActual = BatState.Patrullando;

    void Start()
    {
        startPos = transform.position;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

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
        }
    }

    void Patrullar()
    {
        float limiteDer = startPos.x + patrolDistance;
        float limiteIzq = startPos.x - patrolDistance;

        if (movingRight)
        {
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
            transform.localScale = new Vector3(-1, 1, 1);
            if (transform.position.x >= limiteDer) movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime);
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
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, diveSpeed * Time.deltaTime);

        // Mirar al jugador mientras baja
        if (playerTransform.position.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);

        // Si falla el ataque y pasa de largo hacia el piso, se pone a recuperarse
        if (transform.position.y < playerTransform.position.y - 0.5f)
        {
            IniciarRecuperacion();
        }
    }

    // --- NUEVA FUNCIÓN: ESPERAR 2 SEGUNDOS ---
    void Recuperarse()
    {
        cooldownTimer -= Time.deltaTime; // Descontamos el tiempo

        // Mientras espera, lo hacemos volar y seguirte un poco por encima de tu cabeza
        Vector2 hoverPosition = new Vector2(playerTransform.position.x, playerTransform.position.y + hoverHeight);
        transform.position = Vector2.MoveTowards(transform.position, hoverPosition, patrolSpeed * Time.deltaTime);

        // Que te siga mirando mientras vuela sobre vos
        if (playerTransform.position.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);

        // Si se le acabó el tiempo, ¡vuelve a atacar!
        if (cooldownTimer <= 0)
        {
            estadoActual = BatState.Atacando;
        }
    }

    void IniciarRecuperacion()
    {
        estadoActual = BatState.Recuperando;
        cooldownTimer = attackCooldown; // Configuramos los 2 segundos
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null && estadoActual == BatState.Atacando)
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
        transform.position = new Vector2(transform.position.x + direction.x * 0.2f, transform.position.y + direction.y * 0.2f);
        if (health <= 0) Destroy(gameObject);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, detectionRadius);
    }
}
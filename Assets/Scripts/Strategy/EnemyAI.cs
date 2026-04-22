using UnityEngine;

// EL CONTEXTO (El cerebro que usa las estrategias)
public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolRange = 5f;

    [Header("Detección")]
    public float detectionRange = 5f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    [Header("Sensores")]
    public float groundCheckDistance = 0.5f;
    public float wallCheckDistance = 0.5f;
    public Transform sensorOrigin;

    // Variables públicas para que las estrategias las lean
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform player;
    [HideInInspector] public Vector2 startPosition;
    [HideInInspector] public bool movingRight = true;
    [HideInInspector] public float lastTurnTime;
    public float turnCooldown = 0.5f;

    private IEnemyStrategy currentStrategy;
    private PatrolStrategy patrolStrategy = new PatrolStrategy();
    private ChaseStrategy chaseStrategy = new ChaseStrategy();
    private Enemy enemyComponent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyComponent = GetComponent<Enemy>();
        startPosition = transform.position;
        Physics2D.queriesStartInColliders = false;

        // Empezamos con la estrategia de patrulla
        currentStrategy = patrolStrategy;
    }

    private void Update()
    {
        if (enemyComponent != null && enemyComponent.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        DetectPlayer();
        
        // Ejecutamos la estrategia que esté activa en este momento
        currentStrategy.Execute(this);
    }

    private void DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerCollider != null)
        {
            player = playerCollider.transform;
            currentStrategy = chaseStrategy; // Cambiamos la estrategia a Persecución
        }
        else
        {
            player = null;
            currentStrategy = patrolStrategy; // Cambiamos la estrategia a Patrulla
        }
    }

    public void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftPoint = new Vector3(startPosition.x - patrolRange, transform.position.y, 0);
            Vector3 rightPoint = new Vector3(startPosition.x + patrolRange, transform.position.y, 0);
            Gizmos.DrawLine(leftPoint, rightPoint);
        }

        if (sensorOrigin != null)
        {
            Gizmos.color = Color.blue;
            float moveDir = movingRight ? 1f : -1f;
            Vector2 sensorPos = sensorOrigin.position;
            Gizmos.DrawRay(sensorPos, Vector2.down * groundCheckDistance);
            Gizmos.DrawRay(sensorPos, (Vector2.right * moveDir) * wallCheckDistance);
        }
    }
}

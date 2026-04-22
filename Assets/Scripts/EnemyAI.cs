using UnityEngine;

// Este script controla el comportamiento (IA) del enemigo: patrulla y persecución.
// Requiere que el objeto tenga un Rigidbody2D y el script Enemy.cs ya creado.
public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    // Velocidad a la que el enemigo camina mientras patrulla.
    public float patrolSpeed = 2f;
    // Velocidad a la que el enemigo corre cuando persigue al jugador.
    public float chaseSpeed = 4f;
    // Distancia máxima que el enemigo puede alejarse de su punto de inicio antes de dar la vuelta.
    public float patrolRange = 5f;

    [Header("Detección")]
    // Distancia a la que el enemigo puede detectar al jugador.
    public float detectionRange = 5f;
    // Capa (Layer) en la que se encuentra el jugador para poder identificarlo.
    public LayerMask playerLayer;
    // Capa de las plataformas para detectar bordes y paredes.
    public LayerMask groundLayer;

    [Header("Sensores de Patrulla")]
    // Distancia del rayo para detectar el suelo frente al enemigo (evita caer de plataformas).
    public float groundCheckDistance = 0.5f;
    // Distancia del rayo para detectar paredes frente al enemigo.
    public float wallCheckDistance = 0.5f;
    // Punto desde donde salen los rayos de detección (generalmente frente a los pies).
    public Transform sensorOrigin;

    // Variables internas de estado
    private Rigidbody2D rb;
    private Transform player;
    private bool isChasing = false;
    private bool movingRight = true;
    private Enemy enemyComponent;
    private float lastTurnTime; // Para evitar giros infinitos (zig-zag)
    private float turnCooldown = 0.5f; // Tiempo mínimo entre giros
    private Vector2 startPosition; // Posición inicial para calcular el rango de patrulla

    private void Awake()
    {
        // Obtenemos las referencias necesarias al iniciar.
        rb = GetComponent<Rigidbody2D>();
        enemyComponent = GetComponent<Enemy>();
        
        // Guardamos la posición inicial
        startPosition = transform.position;

        // Evita que los rayos detecten al propio enemigo (muy importante para el zig-zag)
        Physics2D.queriesStartInColliders = false;
    }

    private void Update()
    {
        // Si el enemigo está muerto (según el script Enemy.cs), no hacemos nada.
        if (enemyComponent != null && enemyComponent.IsDead) return;

        // 1. Buscar al jugador dentro del rango de detección.
        DetectPlayer();

        // 2. Ejecutar el comportamiento según el estado actual.
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // Lógica para detectar si el jugador está cerca.
    private void DetectPlayer()
    {
        // Crea un círculo invisible para detectar objetos en la capa "playerLayer".
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerCollider != null)
        {
            player = playerCollider.transform;
            isChasing = true;
        }
        else
        {
            // Si el jugador se aleja más allá del rango, dejamos de perseguir.
            isChasing = false;
            player = null;
        }
    }

    // Comportamiento de Patrulla: camina hasta un borde o pared y cambia de dirección.
    private void Patrol()
    {
        // Calculamos la dirección actual.
        float moveDir = movingRight ? 1f : -1f;
        
        // Aplicamos la velocidad de patrulla al Rigidbody2D.
        rb.linearVelocity = new Vector2(moveDir * patrolSpeed, rb.linearVelocity.y);

        // Si giramos hace muy poco, no volvemos a girar (evita zig-zag)
        if (Time.time < lastTurnTime + turnCooldown) return;

        // --- DETECCIÓN DE BORDES Y PAREDES ---
        Vector2 sensorPos = sensorOrigin != null ? (Vector2)sensorOrigin.position : (Vector2)transform.position;
        
        // Rayo hacia abajo para detectar suelo
        RaycastHit2D groundInfo = Physics2D.Raycast(sensorPos, Vector2.down, groundCheckDistance, groundLayer);
        
        // Rayo hacia adelante para detectar paredes
        RaycastHit2D wallInfo = Physics2D.Raycast(sensorPos, Vector2.right * moveDir, wallCheckDistance, groundLayer);

        // --- CÁLCULO DE DISTANCIA DE PATRULLA ---
        float distanceFromStart = transform.position.x - startPosition.x;
        bool outOfRange = false;

        if (movingRight && distanceFromStart >= patrolRange)
        {
            outOfRange = true;
        }
        else if (!movingRight && distanceFromStart <= -patrolRange)
        {
            outOfRange = true;
        }

        // Si no hay suelo adelante O hay una pared O se pasó del rango de distancia, giramos.
        if (groundInfo.collider == null || wallInfo.collider != null || outOfRange)
        {
            if (groundInfo.collider == null) Debug.Log("<color=cyan>Giro por: FALTA DE SUELO</color>");
            if (wallInfo.collider != null) Debug.Log("<color=orange>Giro por: PARED DETECTADA (" + wallInfo.collider.name + ")</color>");
            if (outOfRange) Debug.Log("<color=yellow>Giro por: LÍMITE DE DISTANCIA ALCANZADO</color>");

            Flip();
            lastTurnTime = Time.time; // Registramos el momento del giro
        }
    }

    // Comportamiento de Persecución: se mueve directamente hacia la posición del jugador.
    private void ChasePlayer()
    {
        if (player == null) return;

        // Calculamos la dirección hacia el jugador.
        float direction = player.position.x - transform.position.x;
        float moveDir = direction > 0 ? 1f : -1f;

        // Aplicamos la velocidad de persecución.
        rb.linearVelocity = new Vector2(moveDir * chaseSpeed, rb.linearVelocity.y);

        // Orientamos al enemigo hacia donde está corriendo.
        if ((moveDir > 0 && !movingRight) || (moveDir < 0 && movingRight))
        {
            Flip();
        }
    }

    // Gira al enemigo 180 grados horizontalmente.
    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // Dibuja el rango de detección en el editor de Unity para facilitar la configuración.
    private void OnDrawGizmosSelected()
    {
        // Rango de detección (Círculo Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de patrulla (Línea Amarilla)
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
            // Dibujamos el rayo de detección de suelo (coincidiendo con la lógica real)
            Gizmos.DrawRay(sensorPos, Vector2.down * groundCheckDistance);
            // Dibujamos el rayo de detección de pared.
            Gizmos.DrawRay(sensorPos, (Vector2.right * moveDir) * wallCheckDistance);
        }
    }
}

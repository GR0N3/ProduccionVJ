using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolRange = 10f;

    [Header("Detección General")]
    public float detectionRange = 5f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    [Header("Sensores Espaciales")]
    public float groundCheckDistance = 1.0f;
    public float wallCheckDistance = 0.5f;
    public Transform sensorOrigin;

    [Header("Mecánica de Escudo")]
    public float shieldDuration = 2f;
    public float shieldCooldown = 6f;
    private float lastShieldTime = -100f;

    [Header("Ataque 1: Horizontal")]
    public Vector2 horizontalHitboxSize = new Vector2(2f, 1f);
    public Vector2 horizontalHitboxOffset = new Vector2(1f, 0f);
    public float attack1Delay = 0.5f;

    [Header("Ataque 2: Omnidireccional (X e Y)")]
    public float omniHitboxRadius = 2.5f;
    public Vector2 omniHitboxOffset = new Vector2(0f, 0f);
    public float attack2Delay = 0.7f;

    [Header("Tiempos de Combate")]
    public float attackTriggerDistance = 1.8f;
    public float attackCooldown = 2f;

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
    private Animator animator;

    private bool isAttacking = false;
    private float lastAttackTime = -100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyComponent = GetComponent<Enemy>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        Physics2D.queriesStartInColliders = false;

        if (sensorOrigin == null)
        {
            Transform foundSensor = transform.Find("SensorOrigin");
            if (foundSensor != null) sensorOrigin = foundSensor;
        }

        currentStrategy = patrolStrategy;
    }

    private void Update()
    {
        if (enemyComponent != null && (enemyComponent.IsDead || enemyComponent.isStunned))
        {
            if (enemyComponent.isStunned && !enemyComponent.IsDead && !enemyComponent.isBlocking)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        // Si el enemigo está en medio de un ataque o cubriéndose con el escudo, se pausa la IA
        if (isAttacking || enemyComponent.isBlocking) return;

        DetectPlayerAndDecideAction();

        if (!isAttacking && !enemyComponent.isBlocking && currentStrategy != null)
        {
            currentStrategy.Execute(this);
        }
    }

    private void DetectPlayerAndDecideAction()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerCollider != null)
        {
            player = playerCollider.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 1. Prioridad Máxima: Atacar si está en rango y el cooldown lo permite
            if (distanceToPlayer <= attackTriggerDistance && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackDecisionRoutine());
                return; // Corta la ejecución para no evaluar el escudo ni la persecución
            }

            // 2. Prioridad Secundaria: Si está cerca pero no puede atacar, evalúa usar el escudo
            if (Time.time >= lastShieldTime + shieldCooldown)
            {
                StartCoroutine(ShieldRoutine());
                return;
            }

            // 3. Prioridad Base: Perseguir
            currentStrategy = chaseStrategy;
        }
        else
        {
            player = null;
            currentStrategy = patrolStrategy;
        }
    }

    // --- RUTINA DE DEFENSA (ESCUDO) ---
    private IEnumerator ShieldRoutine()
    {
        enemyComponent.isBlocking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (animator != null) animator.SetBool("IsBlocking", true);

        // Espera el tiempo configurado para la duración del escudo
        yield return new WaitForSeconds(shieldDuration);

        // Limpieza de estado
        if (animator != null) animator.SetBool("IsBlocking", false);
        enemyComponent.isBlocking = false;
        lastShieldTime = Time.time;
    }

    // --- RUTINA DE ATAQUE (CON VARIANTES) ---
    private IEnumerator AttackDecisionRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Asegurar que mire al jugador antes de bloquear sus movimientos
        if ((player.position.x > transform.position.x && !movingRight) ||
            (player.position.x < transform.position.x && movingRight))
        {
            Flip();
        }

        // Selección aleatoria: 0 para Horizontal, 1 para Omnidireccional
        int tipoAtaque = Random.Range(0, 2);

        if (tipoAtaque == 0)
        {
            if (animator != null) animator.SetTrigger("AttackHorizontal");
            yield return new WaitForSeconds(attack1Delay);
            ProcesarImpactoHorizontal();
        }
        else
        {
            if (animator != null) animator.SetTrigger("AttackOmni");
            yield return new WaitForSeconds(attack2Delay);
            ProcesarImpactoOmnidireccional();
        }

        yield return new WaitForSeconds(0.2f); // Pausa visual de finalización

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // --- CÁLCULO DE ÁREA HORIZONTAL ---
    private void ProcesarImpactoHorizontal()
    {
        if (enemyComponent.IsDead || enemyComponent.isStunned) return;

        float multiplicadorDireccion = movingRight ? 1f : -1f;
        Vector2 centroCaja = (Vector2)transform.position + new Vector2(horizontalHitboxOffset.x * multiplicadorDireccion, horizontalHitboxOffset.y);

        Collider2D impacto = Physics2D.OverlapBox(centroCaja, horizontalHitboxSize, 0f, playerLayer);

        AplicarDanoSiImpacta(impacto);
    }

    // --- CÁLCULO DE ÁREA OMNIDIRECCIONAL (X e Y) ---
    private void ProcesarImpactoOmnidireccional()
    {
        if (enemyComponent.IsDead || enemyComponent.isStunned) return;

        Vector2 centroCirculo = (Vector2)transform.position + omniHitboxOffset;

        Collider2D impacto = Physics2D.OverlapCircle(centroCirculo, omniHitboxRadius, playerLayer);

        AplicarDanoSiImpacta(impacto);
    }

    private void AplicarDanoSiImpacta(Collider2D impacto)
    {
        if (impacto != null)
        {
            PlayerController playerCtrl = impacto.GetComponent<PlayerController>();
            if (playerCtrl != null && !playerCtrl.isDead)
            {
                Vector2 direccionEmpuje = (impacto.transform.position - transform.position).normalized;
                direccionEmpuje.y = 0.5f;
                playerCtrl.TakeDamage(enemyComponent.damageToPlayer, direccionEmpuje, enemyComponent.knockbackToPlayer);
            }
        }
    }

    public void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // --- DIBUJADO DE GIZMOS PARA CONFIGURACIÓN VISUAL ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Radio de activación (Cuándo decide frenar y tirar un ataque)
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackTriggerDistance);

        // Visualización del Ataque Horizontal (Caja Azul)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        float moveDir = movingRight ? 1f : -1f;
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(horizontalHitboxOffset.x * moveDir, horizontalHitboxOffset.y);
        Gizmos.DrawWireCube(boxCenter, horizontalHitboxSize);

        // Visualización del Ataque Omnidireccional (Círculo Magenta)
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Vector2 circleCenter = (Vector2)transform.position + omniHitboxOffset;
        Gizmos.DrawWireSphere(circleCenter, omniHitboxRadius);

        if (sensorOrigin != null)
        {
            Gizmos.color = Color.blue;
            Vector2 sensorPos = sensorOrigin.position;
            Gizmos.DrawRay(sensorPos, Vector2.down * groundCheckDistance);
            Gizmos.DrawRay(sensorPos, (Vector2.right * moveDir) * wallCheckDistance);
        }
    }
}
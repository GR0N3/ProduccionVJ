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
    [Tooltip("Asegurate de poner acá la capa de tus paredes/suelo para que bloqueen la visión")]
    public LayerMask groundLayer;

    [Header("Sensores Espaciales")]
    public float groundCheckDistance = 1.0f;
    public float wallCheckDistance = 0.5f;
    public Transform sensorOrigin;

    [Header("Ataque 1: Horizontal")]
    public Vector2 horizontalHitboxSize = new Vector2(2f, 1f);
    public Vector2 horizontalHitboxOffset = new Vector2(1f, 0f);
    public float attack1Delay = 0.3f;
    public float attack1Duration = 0.2f;

    [Header("Ataque 2: Omnidireccional (X e Y)")]
    public float omniHitboxRadius = 2.5f;
    public Vector2 omniHitboxOffset = new Vector2(0f, 0f);
    public float attack2Delay = 0.3f;
    public float attack2Duration = 0.2f;

    [Header("Tiempos de Combate")]
    public float attackTriggerDistance = 1.5f;
    public float shieldDropRecovery = 0.6f;
    public float postAttackPause = 0.8f;
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
    private float lastShieldDropTime = -100f;

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
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        DetectPlayerAndDecideAction();

        if (enemyComponent.isBlocking || currentStrategy == null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            GirarHaciaElJugador();
        }
        else if (currentStrategy != null)
        {
            currentStrategy.Execute(this);
        }

        if (animator != null && !enemyComponent.isBlocking && !isAttacking && currentStrategy != null)
        {
            bool seMueve = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            animator.SetBool("IsMoving", seMueve);
        }
    }

    // --- SISTEMA DE VISIÓN ACTUALIZADO CON RAYCAST ---
    private void DetectPlayerAndDecideAction()
    {
        // 1. El radar circular detecta si estás a menos de X metros
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerCollider != null)
        {
            player = playerCollider.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 2. Lanza el láser visual hacia tu personaje
            Vector2 direccionAlJugador = (player.position - transform.position).normalized;
            RaycastHit2D paredEnElMedio = Physics2D.Raycast(transform.position, direccionAlJugador, distanceToPlayer, groundLayer);

            // 3. Si el láser NO choca contra una pared, te está viendo
            if (paredEnElMedio.collider == null)
            {
                // ZONA DE COMBATE (Estás Cerca)
                if (distanceToPlayer <= attackTriggerDistance)
                {
                    if (enemyComponent.isBlocking)
                    {
                        DesactivarEscudo();
                        lastShieldDropTime = Time.time;
                    }

                    if (Time.time < lastShieldDropTime + shieldDropRecovery)
                    {
                        currentStrategy = null;
                    }
                    else
                    {
                        if (Time.time >= lastAttackTime + attackCooldown)
                        {
                            StartCoroutine(AttackDecisionRoutine());
                        }
                        else
                        {
                            currentStrategy = chaseStrategy;
                        }
                    }
                }
                // ZONA DE DEFENSA (Estás lejos)
                else
                {
                    ActivarEscudo();
                    currentStrategy = null;
                }
            }
            // 4. Si el láser chocó con una pared (hay un obstáculo en el medio)
            else
            {
                // Actúa como si no existieras, sigue su patrulla normal
                player = null;
                DesactivarEscudo();
                currentStrategy = patrolStrategy;
            }
        }
        else
        {
            // ZONA PACÍFICA (Saliste de su radar)
            player = null;
            DesactivarEscudo();
            currentStrategy = patrolStrategy;
        }
    }

    private void ActivarEscudo()
    {
        if (!enemyComponent.isBlocking)
        {
            enemyComponent.isBlocking = true;
            if (animator != null) animator.SetBool("IsBlocking", true);
        }
    }

    private void DesactivarEscudo()
    {
        if (enemyComponent.isBlocking)
        {
            enemyComponent.isBlocking = false;
            if (animator != null) animator.SetBool("IsBlocking", false);
        }
    }

    private void GirarHaciaElJugador()
    {
        if (player != null)
        {
            if ((player.position.x > transform.position.x && !movingRight) ||
                (player.position.x < transform.position.x && movingRight))
            {
                Flip();
            }
        }
    }

    private IEnumerator AttackDecisionRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (animator != null) animator.SetBool("IsMoving", false);

        GirarHaciaElJugador();

        int tipoAtaque = Random.Range(0, 2);

        if (tipoAtaque == 0)
        {
            if (animator != null) animator.SetTrigger("AttackHorizontal");
            yield return new WaitForSeconds(attack1Delay);
            yield return StartCoroutine(ProcesarImpactoHorizontalContinuo(attack1Duration));
        }
        else
        {
            if (animator != null) animator.SetTrigger("AttackOmni");
            yield return new WaitForSeconds(attack2Delay);
            yield return StartCoroutine(ProcesarImpactoOmniContinuo(attack2Duration));
        }

        if (animator != null)
        {
            animator.ResetTrigger("AttackHorizontal");
            animator.ResetTrigger("AttackOmni");
        }

        yield return new WaitForSeconds(postAttackPause);

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private IEnumerator ProcesarImpactoHorizontalContinuo(float duracionDelGolpe)
    {
        float tiempo = 0f;
        bool yaGolpeo = false;

        while (tiempo < duracionDelGolpe && !yaGolpeo)
        {
            if (enemyComponent.IsDead || enemyComponent.isStunned) yield break;

            float multiplicadorDireccion = movingRight ? 1f : -1f;
            Vector2 centroCaja = (Vector2)transform.position + new Vector2(horizontalHitboxOffset.x * multiplicadorDireccion, horizontalHitboxOffset.y);

            Collider2D impacto = Physics2D.OverlapBox(centroCaja, horizontalHitboxSize, 0f, playerLayer);

            if (impacto != null)
            {
                PlayerController playerCtrl = impacto.GetComponent<PlayerController>();
                if (playerCtrl != null && !playerCtrl.isDead)
                {
                    Vector2 direccionEmpuje = (impacto.transform.position - transform.position).normalized;
                    direccionEmpuje.y = 0.5f;
                    playerCtrl.TakeDamage(enemyComponent.damageToPlayer, direccionEmpuje, enemyComponent.knockbackToPlayer);
                    yaGolpeo = true;
                }
            }

            tiempo += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ProcesarImpactoOmniContinuo(float duracionDelGolpe)
    {
        float tiempo = 0f;
        bool yaGolpeo = false;

        while (tiempo < duracionDelGolpe && !yaGolpeo)
        {
            if (enemyComponent.IsDead || enemyComponent.isStunned) yield break;

            Vector2 centroCirculo = (Vector2)transform.position + omniHitboxOffset;
            Collider2D impacto = Physics2D.OverlapCircle(centroCirculo, omniHitboxRadius, playerLayer);

            if (impacto != null)
            {
                PlayerController playerCtrl = impacto.GetComponent<PlayerController>();
                if (playerCtrl != null && !playerCtrl.isDead)
                {
                    Vector2 direccionEmpuje = (impacto.transform.position - transform.position).normalized;
                    direccionEmpuje.y = 0.5f;
                    playerCtrl.TakeDamage(enemyComponent.damageToPlayer, direccionEmpuje, enemyComponent.knockbackToPlayer);
                    yaGolpeo = true;
                }
            }

            tiempo += Time.deltaTime;
            yield return null;
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

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackTriggerDistance);

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        float moveDir = movingRight ? 1f : -1f;
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(horizontalHitboxOffset.x * moveDir, horizontalHitboxOffset.y);
        Gizmos.DrawWireCube(boxCenter, horizontalHitboxSize);

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
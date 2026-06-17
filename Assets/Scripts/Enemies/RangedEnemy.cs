using System.Collections;
using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    public float distanciaEvasionPared = 0.5f;

    [Header("Detección y Visión")]
    [Tooltip("Distancia a la que te detecta. Ponelo en 8 o 10.")]
    public float attackRange = 8f;
    public LayerMask playerLayer;
    [Tooltip("Capas de paredes y piso para que no te vea a través de ellas.")]
    public LayerMask groundLayer;

    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab;
    [Tooltip("Un objeto vacío hijo del enemigo, ubicado en su mano o arco.")]
    public Transform firePoint;
    public float projectileSpeed = 10f;

    [Header("Game Feel (Tiempos de Animación)")]
    [Tooltip("Cuánto tarda en salir la flecha desde que empieza la animación.")]
    public float attackAnticipation = 0.4f;
    [Tooltip("Cuánto tiempo se queda quieto después de disparar.")]
    public float postAttackPause = 1f;
    public float attackCooldown = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private Enemy enemyComponent;
    private Transform player;

    private Vector2 startPosition;
    private bool movingRight = true;
    private bool isAttacking = false;
    private float lastAttackTime = -100f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyComponent = GetComponent<Enemy>();
        startPosition = transform.position;

        // Esto arregla el bug de la visión ciega
        Physics2D.queriesStartInColliders = false;
    }

    void Update()
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

        BuscarJugador();

        if (player == null) Patrullar();

        if (animator != null)
        {
            bool seMueve = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            animator.SetBool("IsMoving", seMueve);
        }
    }

    private void BuscarJugador()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);

        if (hit != null)
        {
            Vector2 direccionAlJugador = (hit.transform.position - transform.position).normalized;
            float distancia = Vector2.Distance(transform.position, hit.transform.position);
            RaycastHit2D paredEnElMedio = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, groundLayer);

            if (paredEnElMedio.collider == null)
            {
                player = hit.transform;
                GirarHaciaJugador();

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(RutinaDisparo());
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
            else
            {
                player = null;
            }
        }
        else
        {
            player = null;
        }
    }

    private IEnumerator RutinaDisparo()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Vector3 posicionObjetivo = player.position;

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackAnticipation);

        // --- CREACIÓN DEL PROYECTIL ---
        if (enemyComponent != null && !enemyComponent.IsDead && projectilePrefab != null)
        {
            // Si te olvidaste el FirePoint, lo tira desde el centro de su cuerpo para que no se rompa el juego
            Vector3 puntoDeDisparo = firePoint != null ? firePoint.position : transform.position;

            Vector2 direccionDisparo = (posicionObjetivo - puntoDeDisparo).normalized;

            GameObject bala = Instantiate(projectilePrefab, puntoDeDisparo, Quaternion.identity);

            float angulo = Mathf.Atan2(direccionDisparo.y, direccionDisparo.x) * Mathf.Rad2Deg;
            bala.transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

            Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
            if (rbBala != null)
            {
                rbBala.linearVelocity = direccionDisparo * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(postAttackPause);

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void Patrullar()
    {
        float limiteDer = startPosition.x + patrolDistance;
        float limiteIzq = startPosition.x - patrolDistance;

        Vector2 direccionMirada = movingRight ? Vector2.right : Vector2.left;
        bool chocaPared = Physics2D.Raycast(transform.position, direccionMirada, distanciaEvasionPared, groundLayer);

        if (chocaPared) Flip();
        else if (movingRight)
        {
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);
            if (transform.position.x >= limiteDer) Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);
            if (transform.position.x <= limiteIzq) Flip();
        }
    }

    private void GirarHaciaJugador()
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

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
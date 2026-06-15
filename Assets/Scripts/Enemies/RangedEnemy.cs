using System.Collections;
using UnityEngine;

public class RangedEnemy : MonoBehaviour, IDamageable
{
    [Header("Estadísticas")]
    public int health = 3;

    [Header("Patrulla")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    public LayerMask capaObstaculos;
    public float distanciaEvasion = 1f;

    [Header("Detección y Rango")]
    public float detectionRadius = 8f;
    public float loseInterestRadius = 14f;

    [Header("Ataque a Distancia")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 2f;
    public int shotsPerBurst = 3; // Disparos al hilo
    public float timeBetweenShots = 0.5f; // Tiempo entre bala y bala

    [Header("Retroceso Táctico")]
    public float retreatSpeed = 3f;
    public float retreatDuration = 1.5f; // Tiempo que pasa caminando hacia atrás

    private Vector2 startPos;
    private bool movingRight = true;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private bool estaDisparando = false;
    private float tiempoRetrocesoActual;

    private enum RangedState { Patrullando, Atacando, Retrocediendo, Regresando }
    private RangedState estadoActual = RangedState.Patrullando;

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

        // Comprobación para ver si el jugador huyó muy lejos
        if (estadoActual == RangedState.Atacando || estadoActual == RangedState.Retrocediendo)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > loseInterestRadius)
            {
                if (!estaDisparando) estadoActual = RangedState.Regresando;
            }
        }

        switch (estadoActual)
        {
            case RangedState.Patrullando:
                Patrullar();
                BuscarJugador();
                break;
            case RangedState.Atacando:
                if (!estaDisparando)
                {
                    StartCoroutine(RutinaDisparoRafaga());
                }
                break;
            case RangedState.Retrocediendo:
                RetrocederTacticamente();
                break;
            case RangedState.Regresando:
                RegresarACasa();
                BuscarJugador();
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
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(-1, 1, 1); // Mirar Derecha
            if (transform.position.x >= limiteDer) movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(1, 1, 1); // Mirar Izquierda
            if (transform.position.x <= limiteIzq) movingRight = true;
        }
    }

    void BuscarJugador()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius)
        {
            estadoActual = RangedState.Atacando;
        }
    }

    // --- LA MAGIA DE LA RÁFAGA (CORRUTINA) ---
    IEnumerator RutinaDisparoRafaga()
    {
        estaDisparando = true;

        // Frena al enemigo en el lugar para poder disparar cómodo
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Breve pausa para que el jugador reaccione antes del primer tiro
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < shotsPerBurst; i++)
        {
            MirarAlJugador();

            if (bulletPrefab != null && firePoint != null)
            {
                // Instancia la bala
                GameObject bala = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                // Determina la dirección (En tu juego: escala x < 0 es derecha)
                int dirAtaque = transform.localScale.x < 0 ? 1 : -1;
                bala.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(dirAtaque * bulletSpeed, 0);
            }

            // Espera el tiempo entre disparos
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // Terminó de disparar la ráfaga. Prepara el reloj de retroceso.
        tiempoRetrocesoActual = retreatDuration;
        estadoActual = RangedState.Retrocediendo;
        estaDisparando = false;
    }

    // --- EL PASO HACIA ATRÁS ---
    void RetrocederTacticamente()
    {
        MirarAlJugador(); // Retrocede pero sin darte la espalda
        tiempoRetrocesoActual -= Time.fixedDeltaTime;

        // Calcula hacia dónde es "atrás"
        float direccionHuir = transform.position.x > playerTransform.position.x ? 1f : -1f;

        // Verifica si hay pared atrás para no quedarse caminando contra la pared
        Vector2 direccionRayo = new Vector2(direccionHuir, 0);
        bool paredAtras = Physics2D.Raycast(transform.position, direccionRayo, distanciaEvasion, capaObstaculos);

        if (!paredAtras)
        {
            rb.linearVelocity = new Vector2(direccionHuir * retreatSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Se clava en la pared
        }

        // Si se le acaba el tiempo de retroceder, frena y vuelve a disparar
        if (tiempoRetrocesoActual <= 0)
        {
            estadoActual = RangedState.Atacando;
        }
    }

    void RegresarACasa()
    {
        MirarHacia(startPos.x);
        float direccion = startPos.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccion * patrolSpeed, rb.linearVelocity.y);

        if (Vector2.Distance(transform.position, startPos) < 0.5f)
        {
            estadoActual = RangedState.Patrullando;
        }
    }

    void MirarAlJugador() { MirarHacia(playerTransform.position.x); }

    void MirarHacia(float objetivoX)
    {
        if (objetivoX > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    public bool TakeDamage(int damageTaken, Vector2 direction, float knockback)
    {
        health -= damageTaken;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockback, ForceMode2D.Impulse);

        // Si le pegas, interrumpimos todo y lo forzamos a retroceder para que se defienda
        if (!estaDisparando)
        {
            tiempoRetrocesoActual = retreatDuration / 2f;
            estadoActual = RangedState.Retrocediendo;
        }

        if (health <= 0) Destroy(gameObject);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPos != Vector2.zero ? startPos : (Vector2)transform.position, loseInterestRadius);
    }
}
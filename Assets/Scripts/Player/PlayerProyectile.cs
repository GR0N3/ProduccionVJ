using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Colisiones Básicas")]
    [SerializeField] private LayerMask hitMask;

    [Header("Autoseguimiento (Modo Misil)")]
    [Tooltip("Hacelo bien grande (Ej: 8 o 10) para que fije la mira apenas disparás")]
    public float radioDeteccion = 10f;
    [Tooltip("Velocidad de giro. Arriba de 15 es prácticamente inesquivable.")]
    public float velocidadGiro = 20f;
    [Tooltip("¡MUY IMPORTANTE! Si esto dice 'Nothing', nunca va a seguir a nadie. Poné tu capa de enemigos.")]
    public LayerMask capaEnemigos;

    private int damage;
    private float knockbackForce;
    private float currentLifeTime;
    private Vector2 direction;
    private Camera cam;
    private Rigidbody2D rb;

    private Transform objetivoActual;

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, float lifeTime, int damage, float knockbackforce)
    {
        direction = dir.normalized;
        this.currentLifeTime = lifeTime;
        this.damage = damage;
        this.knockbackForce = knockbackforce;

        objetivoActual = null;
    }

    private void Update()
    {
        currentLifeTime -= Time.deltaTime;

        if (cam == null) cam = Camera.main;
        bool isOutside = cam != null ? IsOutsideCamera(transform.position) : false;

        if (currentLifeTime <= 0 || isOutside)
        {
            DestroyBullet();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < 0.1f) return;

        if (objetivoActual == null)
        {
            BuscarVoladorCercano();
        }
        else
        {
            // Si el murciélago se murió, dejamos de seguirlo
            if (!objetivoActual.gameObject.activeInHierarchy)
            {
                objetivoActual = null;
                return;
            }

            // 1. Dirección exacta hacia el murciélago
            Vector2 direccionDeseada = (objetivoActual.position - transform.position).normalized;

            // 2. MATEMÁTICA AGRESIVA: Obliga a la bala a doblar bruscamente hacia el objetivo
            Vector2 nuevaDireccion = Vector2.Lerp(rb.linearVelocity.normalized, direccionDeseada, Time.fixedDeltaTime * velocidadGiro).normalized;

            // 3. Mantiene la velocidad pero con la nueva dirección
            rb.linearVelocity = nuevaDireccion * currentSpeed;
        }

        // Rota el dibujo para que mire hacia adelante
        float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
    }

    void BuscarVoladorCercano()
    {
        Collider2D[] enemigosCercanos = Physics2D.OverlapCircleAll(transform.position, radioDeteccion, capaEnemigos);
        float distanciaMasCorta = Mathf.Infinity;
        Transform enemigoMasCercano = null;

        foreach (Collider2D enemigo in enemigosCercanos)
        {
            if (enemigo.GetComponent<FlyingEnemy>() != null)
            {
                float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
                if (distancia < distanciaMasCorta)
                {
                    distanciaMasCorta = distancia;
                    enemigoMasCercano = enemigo.transform;
                }
            }
        }

        objetivoActual = enemigoMasCercano;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return;

        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            Vector2 direccionImpacto = rb.linearVelocity.normalized;
            damageable.TakeDamage(damage, direccionImpacto, knockbackForce);
            DestroyBullet();
            return;
        }

        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        rb.linearVelocity = Vector2.zero;
        objetivoActual = null;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1 || vp.z < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
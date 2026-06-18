using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damage = 1;
    public float knockbackForce = 5f;

    [Header("Limpieza")]
    public float lifeTime = 3f;
    [Tooltip("Capas donde la flecha choca y se rompe (Ground, Paredes)")]
    public LayerMask groundLayer;

    private float currentLifeTime;

    private void Start()
    {
        currentLifeTime = lifeTime;
    }

    private void Update()
    {
        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ResetLife()
    {
        currentLifeTime = lifeTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  MODO PARRY: Solo queremos que no lastime al jugador
        if (gameObject.CompareTag("Parried"))
        {
            if (collision.CompareTag("Player")) return;

            // Si al caer toca el piso, se destruye
            if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            {
                Destroy(gameObject);
            }
            return;
        }

        
        if (collision.gameObject.GetComponent<Enemy>() != null || collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                knockbackDir.y = 0.5f;

                player.TakeDamage(damage, knockbackDir, knockbackForce);
                Destroy(gameObject);
            }
        }
        else if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
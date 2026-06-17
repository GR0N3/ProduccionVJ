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

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Si choca contra el enemigo que la tiró u otro monstruo, la flecha los ignora.
        if (collision.gameObject.GetComponent<Enemy>() != null || collision.gameObject.CompareTag("Enemy") || collision.gameObject.GetComponent<RangedEnemyAI>() != null)
        {
            return;
        }

        // 2. Si choca contra el Jugador
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
        // 3. Si choca contra el piso/pared
        else if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
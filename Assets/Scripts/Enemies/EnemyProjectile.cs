using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 5f;
    public float speed = 10f;

    public float lifeTime = 3f;

    [Tooltip("Capas donde la flecha choca y se rompe (Ground, Paredes)")]
    public LayerMask groundLayer;

    private float currentLifeTime;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
        
        // Si al caer toca el piso, se destruye
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();

            Vector2 direction = rb.linearVelocity.normalized;

            player.TakeDamage(damage, direction, knockbackForce);
            Destroy(gameObject);
            
        }
    }
}
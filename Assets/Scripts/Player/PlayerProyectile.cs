using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask; // Solo poner la capa de "Paredes/Suelo" acá

    private int damage;
    private float knockbackForce;
    private float currentLifeTime;
    private Vector2 direction;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Init(Vector2 dir, float lifeTime, int damage, float knockbackforce)
    {
        direction = dir.normalized;
        this.currentLifeTime = lifeTime;
        this.damage = damage;
        this.knockbackForce = knockbackforce;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Seguro para no golpear al propio jugador por accidente
        if (collision.CompareTag("Player")) return;

        IDamageable damageable = collision.GetComponent<IDamageable>();

        // 1. Prioridad: Hacer daño
        if (damageable != null)
        {
            damageable.TakeDamage(damage, direction, knockbackForce);
            DestroyBullet();
            return;
        }

        // 2. Destruirse si toca una pared
        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1 || vp.z < 0;
    }
}
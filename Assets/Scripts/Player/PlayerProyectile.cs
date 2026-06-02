using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;

    private int damage;
    private float knockbackForce;
    private float lifeTime = 1f;
    private float currentLifeTime; // Variable para el temporizador
    private Vector2 direction;

    private Camera cam;

    private void Awake()
    {
        // Asignamos la cámara desde el principio por seguridad
        cam = Camera.main;
    }

    public void Init(Vector2 dir, float lifeTime, int damage, float knockbackforce)
    {
        direction = dir.normalized;
        this.lifeTime = lifeTime;
        this.currentLifeTime = lifeTime; // Iniciamos el temporizador
        this.damage = damage;
        this.knockbackForce = knockbackforce;
    }

    private void Update()
    {
        // Reducimos el tiempo de vida de la bala fotograma a fotograma
        currentLifeTime -= Time.deltaTime;

        // Chequeo de seguridad constante por si la cámara principal se pierde o no tiene el tag
        if (cam == null) cam = Camera.main;

        // Verificamos si salió de la cámara (solo si la cámara existe, si no, es falso)
        bool isOutside = cam != null ? IsOutsideCamera(transform.position) : false;

        // Si el tiempo llega a 0 o sale de la cámara, vuelve a la pool
        if (currentLifeTime <= 0 || isOutside)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();

        // Si choca contra el entorno (paredes/suelo)
        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }

        // Si choca contra un enemigo
        if (damageable != null)
        {
            damageable.TakeDamage(damage, direction, knockbackForce);
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        return
            vp.x < 0 || vp.x > 1 ||
            vp.y < 0 || vp.y > 1 ||
            vp.z < 0;
    }
}
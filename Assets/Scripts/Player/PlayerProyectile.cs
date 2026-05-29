using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class Bullet : MonoBehaviour
{

    [SerializeField] private LayerMask hitMask;

    private int damage;
    private float knockbackForce;
    private float lifeTime = 1f;
    private Vector2 direction;

    private Camera cam;

    public void Init(Vector2 dir, float lifeTime, int damage, float knockbackforce)
    {
        cam = Camera.main;
        direction = dir.normalized;
        this.lifeTime = lifeTime;
        this.damage = damage;
        this.knockbackForce = knockbackforce;
    }

    private void Update()
    {
        if (IsOutsideCamera(transform.position)) 
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        IDamageable damageable = collision.GetComponent<IDamageable>();

        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }

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
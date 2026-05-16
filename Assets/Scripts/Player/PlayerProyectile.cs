using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class Bullet : MonoBehaviour
{

    [SerializeField] private LayerMask hitMask;

    private int damage;
    private float knockbackForce;
    private float lifeTime = 5f;
    private Vector2 direction;

    public void Init(Vector2 dir, float lifeTime, int damage, float knockbackforce)
    {
        direction = dir.normalized;
        this.lifeTime = lifeTime;
        this.damage = damage;
        this.knockbackForce = knockbackforce;
    }

    private void OnEnable()
    {
        StartCoroutine(DefaultStart());
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

    IEnumerator DefaultStart() 
    {
        yield return new WaitForSeconds(lifeTime);

        if (gameObject.activeSelf) 
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}
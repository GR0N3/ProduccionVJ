using Unity.VisualScripting;
using UnityEngine;


public class Bullet : MonoBehaviour
{

    public int damage = 1;

    public float knockbackForce = 5f;

    public float lifeTime = 5f;



    private Vector2 direction;


    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
    }


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }


    public LayerMask hitMask;


    private void OnTriggerEnter2D(Collider2D collision)
    {

        IDamageable damageable = collision.GetComponent<IDamageable>();


        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }

        if (damageable != null)
        {
            damageable.TakeDamage(damage, direction, knockbackForce);
            Destroy(gameObject);
        }
    }
}
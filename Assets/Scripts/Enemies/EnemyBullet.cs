using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 5f;
    public float lifetime = 3f;

    private Vector2 posicionAnterior;

    private void Start()
    {
        posicionAnterior = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // --- SISTEMA ANTI-TÚNEL MATEMÁTICO ---
        Vector2 posicionActual = transform.position;
        Vector2 direccion = posicionActual - posicionAnterior;
        float distancia = direccion.magnitude;

        if (distancia > 0)
        {
            // Dispara un láser invisible desde donde estaba en el frame anterior hasta donde está ahora
            RaycastHit2D hit = Physics2D.Raycast(posicionAnterior, direccion.normalized, distancia);

            // Si el láser cortó algo en el medio del salto de fotogramas, lo procesamos
            if (hit.collider != null)
            {
                ProcesarChoque(hit.collider);
            }
        }

        // Actualizamos la posición para el próximo frame
        posicionAnterior = posicionActual;
    }

    // Por las dudas, mantenemos el Trigger normal como respaldo
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcesarChoque(collision);
    }

    private void ProcesarChoque(Collider2D collision)
    {
        // 1. Ignorar al propio enemigo u otras balas para que no explote al nacer
        if (collision.CompareTag("Enemigo") || collision.gameObject.layer == 9) return;

        // 2. ¿Tocó al jugador?
        //PlayerController player = collision.GetComponent<PlayerController>();
        //if (player != null)
        //{
        //    Vector2 direccionEmpuje = (player.transform.position - transform.position).normalized;
        //    direccionEmpuje.y = 0.5f;

        //    player.TakeDamage(damage, direccionEmpuje, knockbackForce);
        //    Destroy(gameObject);
        //}
        // 3. ¿Tocó una pared o el piso?
        else if (collision.gameObject.layer == 11 || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
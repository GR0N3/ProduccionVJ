using UnityEngine;

// Este script se encarga de la lógica de los proyectiles (cuchillos lanzados).
public class Bullet : MonoBehaviour
{
    // Daño que inflige el proyectil.
    public int damage = 1;
    // Fuerza de empuje que aplica al impactar.
    public float knockbackForce = 5f;
    // Tiempo de vida antes de que el proyectil se destruya solo.
    public float lifeTime = 5f;
    // Puntos otorgados al eliminar a un enemigo.
    public int pointsPerKill = 10;

    private Vector2 direction;

    // Inicializa la dirección del proyectil.
    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        // El proyectil se destruye después de cierto tiempo para no llenar la memoria.
        Destroy(gameObject, lifeTime);
    }

    // Máscara de capas para definir con qué puede chocar el proyectil.
    public LayerMask hitMask;

    // Se ejecuta cuando el proyectil entra en contacto con otro Collider2D.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Intentamos obtener el componente IDamageable (interfaz para daño).
        IDamageable damageable = collision.GetComponent<IDamageable>();

        // Si chocamos con algo que está en la máscara de impacto (paredes, etc.):
        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }

        // Si el objeto con el que chocamos puede recibir daño:
        if (damageable != null)
        {
            // Aplicamos daño y comprobamos si el enemigo murió.
            bool murio = damageable.TakeDamage(damage, direction, knockbackForce);
            
            // Si el enemigo murió a causa de este impacto:
            if (murio)
            {
                // Si el ScoreManager existe en la escena, sumamos los puntos.
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(pointsPerKill);
                }
            }

            // El proyectil se destruye después de impactar a un enemigo.
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

// Este script se encarga de la lógica de ataque del cuchillo.
// Debe estar adjunto al objeto del cuchillo que tenga un Collider2D configurado como "Is Trigger".
public class Knife : MonoBehaviour
{
    // Daño que inflige el cuchillo.
    public int damage = 2;
    // Fuerza de empuje que aplica al enemigo al golpearlo.
    public float knockbackForce = 10f;
    // Puntos que otorga al matar a un enemigo.
    public int pointsPerKill = 10;

    // Se ejecuta cuando el cuchillo entra en el área de otro Collider2D.
    // Se ejecuta al iniciar para confirmar que el script funciona
    private void Start()
    {
        Debug.Log("Script Cuchillo activado en: " + gameObject.name);
    }

    // Por si acaso usas colisión normal en lugar de Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarAtaque(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcesarAtaque(collision);
    }

    private void ProcesarAtaque(Collider2D objetoTocado)
    {
        Debug.Log("Cuchillo detectó impacto con: " + objetoTocado.name);

        IDamageable damageable = objetoTocado.GetComponent<IDamageable>();

        if (damageable != null)
        {
            Vector2 hitDirection = (objetoTocado.transform.position - transform.position).normalized;
            bool murio = damageable.TakeDamage(damage, hitDirection, knockbackForce);
            
            if (murio)
            {
                Debug.Log("<color=yellow>¡Enemigo muerto! Sumando puntos...</color>");
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(pointsPerKill);
                }
            }
        }
    }
}

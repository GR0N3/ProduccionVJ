using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Tooltip("Cuánta vida cura este objeto (Ej: 2 equivale a 1 corazón entero)")]
    public int curacion = 2;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float saltoX = Random.Range(-2f, 2f); // Salto aleatorio a la izquierda o derecha
            float saltoY = Random.Range(3f, 6f);  // Salto hacia arriba
            rb.AddForce(new Vector2(saltoX, saltoY), ForceMode2D.Impulse);
        }
    }

    // Funciona con físicas sólidas (cuando el corazón choca contra el piso o el jugador)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarCurar(collision.gameObject);
    }

    // Lo dejamos por las dudas si alguna vez usás un radar Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IntentarCurar(collision.gameObject);
    }

    private void IntentarCurar(GameObject objetoTocado)
    {
        // Si lo que tocó el corazón es el Jugador...
        if (objetoTocado.CompareTag("Player"))
        {
            var jugador = ServiceLocator.Get<PlayerManager>().PlayerHealth;

            // Si el jugador existe y NO tiene la vida al máximo...
            if (jugador != null && jugador.CurrentHealth < jugador.MaxHealth)
            {
                jugador.GainHealth(curacion);
                Destroy(gameObject); // Destruye el corazón
            }
        }
    }
}
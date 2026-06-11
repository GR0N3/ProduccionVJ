using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Tooltip("Arrastra aquí el objeto vacío que servirá de destino")]
    public Transform puntoDeDestino;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Revisamos de forma 100% segura si el que tocó la puerta es el jugador
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null && puntoDeDestino != null)
        {
            // 1. Movemos al jugador a la nueva posición
            player.transform.position = puntoDeDestino.position;

            // 2. Le frenamos la velocidad a cero para que no salga disparado si entró saltando
            player.rb.linearVelocity = Vector2.zero;
        }
    }
}
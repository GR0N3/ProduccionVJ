using UnityEngine;

public class MiraCamara : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El Rigidbody de tu jugador (si lo dejás vacío, lo busca solo)")]
    public Rigidbody2D rbJugador;

    [Tooltip("Cuánto baja la cámara al caer (Ej: -4)")]
    public float offsetAbajo = -4f;

    [Tooltip("Qué tan rápido baja y sube la cámara")]
    public float velocidad = 3f;

    private Vector3 posicionOriginal;

    void Start()
    {
        // Guardamos su posición inicial (normalmente 0,0,0)
        posicionOriginal = transform.localPosition;

        // Si te olvidaste de asignar al jugador, lo busca automáticamente en el objeto "Padre"
        if (rbJugador == null)
        {
            rbJugador = GetComponentInParent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (rbJugador == null) return;

        // Por defecto, queremos que el señuelo esté en el pecho/centro del jugador
        float destinoY = posicionOriginal.y;

        // Si el jugador está cayendo (velocidad negativa), bajamos el señuelo
        if (rbJugador.linearVelocity.y < -1f)
        {
            destinoY = posicionOriginal.y + offsetAbajo;
        }

        // Movemos el señuelo SUAVEMENTE arriba o abajo
        Vector3 posicionDeseada = new Vector3(posicionOriginal.x, destinoY, posicionOriginal.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, posicionDeseada, Time.deltaTime * velocidad);
    }
}
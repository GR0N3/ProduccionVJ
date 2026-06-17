using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Arrastrá acá a tu Jugador desde la Jerarquía")]
    public Transform jugador;

    [Header("Configuración Base")]
    [Tooltip("Velocidad con la que la cámara persigue al jugador (Ej: 5)")]
    public float suavizado = 5f;
    [Tooltip("Posición base de la cámara. La Z DEBE ser -10 para que se vea el juego.")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Visión de Aterrizaje")]
    [Tooltip("Cuántas unidades bajará la cámara cuando el jugador esté cayendo (Ej: -4)")]
    public float offsetAbajo = -4f;
    [Tooltip("Qué tan rápido hace el paneo para mirar hacia abajo y volver a subir")]
    public float velocidadPaneo = 3f;

    private Rigidbody2D rbJugador;
    private float currentYOffset;

    void Start()
    {
        if (jugador != null)
        {
            rbJugador = jugador.GetComponent<Rigidbody2D>();
        }
        currentYOffset = offset.y; // Inicia con la altura normal
    }

    
    void LateUpdate()
    {
        if (jugador == null) return;

        
        float targetYOffset = offset.y;

        
        if (rbJugador != null && rbJugador.linearVelocity.y < -1f)
        {
            targetYOffset = offset.y + offsetAbajo;
        }

       
        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, Time.deltaTime * velocidadPaneo);

       
        Vector3 posicionDeseada = jugador.position + new Vector3(offset.x, currentYOffset, offset.z);

       
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizado);
    }
}
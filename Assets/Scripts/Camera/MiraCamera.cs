using UnityEngine;

public class CameraScanner : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("¡OJO! Volvé a arrastrar a tu Jugador acá desde la Jerarquía")]
    public PlayerController jugador;
    [Tooltip("Tu capa de piso y paredes (Ground)")]
    public LayerMask capaSuelo;

    [Header("Láser Buscador")]
    [Tooltip("Cuántos metros hacia adelante mira el láser")]
    public float distanciaAdelante = 4f;
    [Tooltip("Poné un número un poco mayor a tu salto máximo (Ej: 8)")]
    public float distanciaAbajo = 8f;

    [Header("Ajustes de Cámara")]
    [Tooltip("Velocidad con la que se mueve la cámara (3 a 5 es ideal)")]
    public float suavidad = 4f;

    void Update()
    {
        if (jugador == null) return;

        // 1. ¿Para dónde está mirando el jugador? (Para disparar el láser)
        float direccionX = jugador.transform.localScale.x > 0 ? 1f : -1f;

        // 2. Vector del láser
        Vector2 vectorBusqueda = new Vector2(direccionX * distanciaAdelante, -distanciaAbajo);

        // 3. Disparamos el láser
        RaycastHit2D hit = Physics2D.Raycast(jugador.transform.position, vectorBusqueda.normalized, vectorBusqueda.magnitude, capaSuelo);

        Vector3 posicionDeseada;

        // ¿El jugador está saltando hacia arriba?
        bool estaSaltandoHaciaArriba = jugador.rb.linearVelocity.y > 0.1f && !jugador.enSuelo;

        if (hit.collider != null || estaSaltandoHaciaArriba)
        {
            
            //  X y Y son EXACTAMENTE la posición del jugador.
            posicionDeseada = new Vector3(jugador.transform.position.x, jugador.transform.position.y, 0f);
        }
        else
        {
           
            posicionDeseada = new Vector3(jugador.transform.position.x, jugador.transform.position.y - distanciaAbajo, 0f);
        }

        // 4. Movemos el señuelo (la cámara) con fluidez
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavidad);
    }

    private void OnDrawGizmos()
    {
        if (jugador == null) return;
        float direccionX = jugador.transform.localScale.x > 0 ? 1f : -1f;
        Vector2 vectorBusqueda = new Vector2(direccionX * distanciaAdelante, -distanciaAbajo);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(jugador.transform.position, vectorBusqueda);
    }
}
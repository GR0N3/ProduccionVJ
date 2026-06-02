using UnityEngine;

public class CameraLeftBorder : MonoBehaviour
{
    private Transform player;
    private Camera cam;

    private float maxX;

    void Start()
    {
        cam = Camera.main;
        // Guardamos la posición inicial donde pusiste el objeto en Unity
        maxX = transform.position.x;
    }

    void LateUpdate()
    {
        // Chequeo de seguridad: asegurarnos de tener la cámara
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Si todavía no tenemos al jugador, intentamos buscarlo
        if (player == null)
        {
            if (SessionController.Instance != null &&
                SessionController.Instance.PlayerManager != null &&
                SessionController.Instance.PlayerManager.PlayerMovement != null)
            {
                player = SessionController.Instance.PlayerManager.PlayerMovement.CurrentPosition;
            }

            // Si después de buscarlo sigue sin existir, cortamos acá para no crashear
            if (player == null) return;
        }

        float halfWidth = cam.orthographicSize * cam.aspect;

        // Calculamos hasta dónde empujó el jugador el punto de la cámara
        float playerPushPoint = player.position.x - halfWidth;

        // Si el jugador avanzó hacia la derecha, el borde avanza con él (no lo deja retroceder)
        if (playerPushPoint > maxX)
        {
            maxX = playerPushPoint;
        }

        // Actualizamos la posición del borde (muro invisible)
        transform.position = new Vector3(maxX, player.position.y, 0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 10,
            transform.position + Vector3.down * 10
        );
    }
}
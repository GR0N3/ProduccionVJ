using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public Transform player;

    [Header("Zona libre")]
    public float deadZone = 2f;

    [Header("Offset hacia adelante")]
    public float forwardOffset = 1.5f;

    private float maxX;

    void Start()
    {
        maxX = transform.position.x;
    }

    void LateUpdate()
    {
        float playerX = player.position.x;

        float triggerX = transform.position.x + deadZone;

        if (playerX > triggerX)
        {
            float targetX = playerX - deadZone + forwardOffset;

            // Evita retroceder
            if (targetX > maxX)
            {
                maxX = targetX;
            }
        }

        transform.position = new Vector3(maxX, player.transform.position.y, transform.position.z);
    }
}
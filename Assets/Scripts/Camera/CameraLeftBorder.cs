using UnityEngine;

public class CameraLeftBorder : MonoBehaviour
{
    public Transform player;
    public Camera cam;

    private float maxX;

    void Start()
    {
        float halfWidth = cam.orthographicSize * cam.aspect;
        maxX = cam.transform.position.x - halfWidth;
    }

    void LateUpdate()
    {
        float halfWidth = cam.orthographicSize * cam.aspect;

        float playerPushPoint = player.position.x - halfWidth;

        if (playerPushPoint > maxX)
        {
            maxX = playerPushPoint;
        }

        transform.position = new Vector3(
            maxX,
            player.position.y,
            0f
        );
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
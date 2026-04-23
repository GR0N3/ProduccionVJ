using UnityEngine;

public class CameraLeftBorder : MonoBehaviour
{
    public Transform player;
    public Camera cam;

    private float maxX;

    void Start()
    {
        float camHalfWidth = cam.orthographicSize * cam.aspect;
        maxX = cam.transform.position.x - camHalfWidth;
    }

    void LateUpdate()
    {
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        float targetX = cam.transform.position.x - camHalfWidth;

        if (targetX > maxX)
        {
            maxX = targetX;
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
using UnityEngine;

public class CameraFollowWithBorder : MonoBehaviour
{
    public Transform player;
    public Transform leftBorder;

    public float followSpeed = 10f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        float halfWidth = cam.orthographicSize * cam.aspect;

        float targetX = leftBorder.position.x + halfWidth;
        float targetY = player.position.y;

        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }
}
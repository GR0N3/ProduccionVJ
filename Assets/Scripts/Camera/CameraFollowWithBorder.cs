using UnityEngine;

public class CameraFollowWithBorder : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform leftBorder;

    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private Vector2 offset;

    [SerializeField] private bool overrideView = true;
    [SerializeField] private float orthographicSize = 5f;
    [SerializeField] private float fieldOfView = 60f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyView();
    }

    private void LateUpdate()
    {
        if (cam == null || player == null || leftBorder == null)
            return;

        ApplyView();

        float halfWidth = cam.orthographicSize * cam.aspect;

        float targetX = leftBorder.position.x + halfWidth + offset.x;
        float targetY = player.position.y + offset.y;

        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }

    private void ApplyView()
    {
        if (!overrideView || cam == null)
            return;

        if (cam.orthographic)
            cam.orthographicSize = orthographicSize;
        else
            cam.fieldOfView = fieldOfView;
    }

    private void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        ApplyView();
    }
}

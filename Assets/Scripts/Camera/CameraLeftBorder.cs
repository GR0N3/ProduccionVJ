using UnityEngine;

public class CameraLeftBorder : MonoBehaviour
{
    private Transform player;
    private Camera cam;
    private PlayerManager playerManager;

    private float maxX;

    private void Awake()
    {
        if (ServiceLocator.TryGet(out PlayerManager registeredPlayerManager))
        {
            playerManager = registeredPlayerManager;
            return;
        }

        playerManager = FindAnyObjectByType<PlayerManager>();

        if (playerManager != null)
        {
            ServiceLocator.Register(playerManager);
        }
    }

    void Start()
    {
        if (playerManager == null)
        {
            return;
        }

        cam = Camera.main;
        player = playerManager.PlayerMovement.CurrentPosition;
        float halfWidth = cam.orthographicSize * cam.aspect;
        maxX = cam.transform.position.x - halfWidth;
    }

    void LateUpdate()
    {
        if (player == null || cam == null)
        {
            return;
        }

        float halfWidth = cam.orthographicSize * cam.aspect;

        float playerPushPoint = player.position.x - halfWidth;

        if (playerPushPoint > maxX)
        {
            maxX = playerPushPoint;
        }

        transform.position = new Vector3( maxX, player.position.y, 0f );
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

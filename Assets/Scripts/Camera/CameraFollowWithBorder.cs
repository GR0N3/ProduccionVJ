using UnityEngine;

public class CameraFollowWithBorder : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform player;
    public Transform leftBorder;

    [Header("Ajustes")]
    public float followSpeed = 10f;
    public float verticalOffset = 1f; 

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("¡Cámara: Jugador encontrado por Tag!");
            }
        }

        
        if (player == null && SessionController.Instance != null && SessionController.Instance.PlayerManager != null)
        {
            if (SessionController.Instance.PlayerManager.PlayerMovement != null)
            {
                player = SessionController.Instance.PlayerManager.PlayerMovement.CurrentPosition;
                Debug.Log("¡Cámara: Jugador encontrado a través del Manager!");
            }
        }
    }

    private void LateUpdate()
    {
        
        if (player == null)
        {
            Debug.LogWarning("La cámara no se mueve porque no encuentra al 'Player'.");
            return;
        }

        float targetX = player.position.x;
        float targetY = player.position.y + verticalOffset;

        
        if (leftBorder != null)
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            float limitLeft = leftBorder.position.x + halfWidth;

            if (targetX < limitLeft)
            {
                targetX = limitLeft;
            }
        }

        
        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Seguimiento")]
    [Tooltip("El Player a seguir. Si lo dejas vacío, lo busca automáticamente.")]
    public Transform target;

    [Tooltip("Tiempo que tarda la cámara en acomodarse (menor = más rápido)")]
    public float smoothTime = 0.25f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Adelanto (Look Ahead)")]
    [Tooltip("Qué tan lejos mira la cámara hacia la dirección en la que caminas")]
    public float lookAheadDistance = 3f;
    [Tooltip("Velocidad con la que la cámara cambia de lado al darte vuelta")]
    public float lookAheadSpeed = 3f;

    private Vector3 velocity = Vector3.zero;
    private float currentLookAheadX;

    private void Start()
    {
        
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        
        float targetLookAheadX = (target.localScale.x > 0) ? lookAheadDistance : -lookAheadDistance;

        
        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadSpeed);

        
        Vector3 targetPosition = target.position + new Vector3(currentLookAheadX, offset.y, offset.z);

        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Arrastra a tu Player aquí desde la Jerarquía")]
    public Transform target;

    [Tooltip("Tiempo que tarda la cámara en acomodarse (menor = más rápido)")]
    public float smoothTime = 0.25f;

    [Tooltip("Qué tan lejos mira la cámara hacia la dirección en la que caminas")]
    public float lookAheadDistance = 3f;

    [Tooltip("Desfase vertical (Y) y profundidad (Z).")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private Vector3 velocity = Vector3.zero;
    private float currentLookAheadX;

    private void LateUpdate()
    {
        if (target == null) return;

        
        if (target.localScale.x > 0)
        {
            currentLookAheadX = lookAheadDistance; 
        }
        else
        {
            currentLookAheadX = -lookAheadDistance; 
        }

       
        Vector3 targetPosition = target.position + new Vector3(currentLookAheadX, offset.y, offset.z);

        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
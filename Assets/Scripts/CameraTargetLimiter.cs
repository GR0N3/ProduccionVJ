using UnityEngine;

public class CameraTargetLimiter : MonoBehaviour
{
    private float maxX;

    void Start()
    {
        maxX = transform.position.x;
    }

    void LateUpdate()
    {
        if (transform.position.x > maxX)
        {
            maxX = transform.position.x;
        }

        transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
    }
}
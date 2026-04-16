using UnityEngine;

public class Door : MonoBehaviour
{
    public Transicion transicion;

    private void OnTriggerEnter2D(Collider2D other)

    {
        if (other.CompareTag("Player"))
        {
            transicion.StarTransicion();
        }
        
    }
}

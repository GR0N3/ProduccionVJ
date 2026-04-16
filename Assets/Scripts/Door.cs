using UnityEngine;

public class Door : MonoBehaviour
{
    public Transicion transicion;

    private void OnTriggerEnter2D(Collider2D other)

    {
        Debug.Log("Algo entro");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Es el player");
            transicion.StarTransicion();
        }

        if (other.CompareTag("Player"))
        {
            transicion.StarTransicion();
        }
        
    }
}

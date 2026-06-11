using UnityEngine;
using TMPro; // Necesario para hablar con la UI

public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("El texto que aparecerá en pantalla al pasar por acá")]
    [TextArea] // Esto hace que la cajita en Unity sea más grande para escribir cómodo
    public string tutorialMessage;

    [Tooltip("Arrastra acá el texto de tu Canvas")]
    public TMP_Text uiText;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Revisamos de forma 100% segura si el que entró es el jugador
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null && uiText != null)
        {
            uiText.text = tutorialMessage;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null && uiText != null)
        {
            // Solo borramos el texto si es el mismo de este cartel 
            // (evita bugs si pasas de un cartel a otro muy rápido)
            if (uiText.text == tutorialMessage)
            {
                uiText.text = ""; // Lo dejamos vacío para que desaparezca
            }
        }
    }
}
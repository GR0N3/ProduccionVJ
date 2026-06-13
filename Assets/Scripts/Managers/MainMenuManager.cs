using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Escribe aquí el nombre exacto de la escena de tu nivel")]
    public string nombreDelNivel = "SampleScene";

    [Header("Opciones")]
    [Tooltip("Arrastra aquí el panel oscuro de Opciones")]
    public GameObject panelDeOpciones;

    public void BotonJugar()
    {
       
        SceneManager.LoadScene(nombreDelNivel);
    }

    public void BotonOpciones()
    {
       
        if (panelDeOpciones != null)
        {
            panelDeOpciones.SetActive(true);
        }
    }

    public void BotonCerrarOpciones()
    {
        
        if (panelDeOpciones != null)
        {
            panelDeOpciones.SetActive(false);
        }
    }

    public void BotonSalir()
    {
        
        Debug.Log("¡Saliendo del juego!");
        Application.Quit();
    }
}
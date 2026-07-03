using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string nombreDelNivel = "Level1";

    [Header("Paneles de Interfaz")]
    public GameObject panelPrincipal;
    public GameObject panelDeOpciones;

    [Header("Ajustes de Sonido")]
    public Slider sliderMusica; 
    public Slider sliderSFX;    

    private void Start()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelDeOpciones != null) panelDeOpciones.SetActive(false);

        // Conectamos el Slider de Música
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(ActualizarVolumenMusica);
        }

        // Conectamos el Slider de Efectos
        if (sliderSFX != null)
        {
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener(ActualizarVolumenSFX);
        }
    }

    public void BotonJugar()
    {
        SceneManager.LoadScene(nombreDelNivel);
    }

    public void BotonOpciones()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);

        if (panelDeOpciones != null)
        {
            panelDeOpciones.SetActive(true);

            if (MusicManager.instance != null)
            {
                // Sincronizamos ambas barritas
                if (sliderMusica != null) sliderMusica.value = MusicManager.instance.audioSource.volume;
                if (sliderSFX != null) sliderSFX.value = MusicManager.instance.volumenSFXActual;
            }
        }
    }

    public void BotonCerrarOpciones()
    {
        if (panelDeOpciones != null) panelDeOpciones.SetActive(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
    }

    public void BotonSalir()
    {
        Application.Quit();
    }

    private void ActualizarVolumenMusica(float valor)
    {
        if (MusicManager.instance != null) MusicManager.instance.CambiarVolumenMusica(valor);
    }

    private void ActualizarVolumenSFX(float valor)
    {
        if (MusicManager.instance != null) MusicManager.instance.CambiarVolumenSFX(valor);
    }
}
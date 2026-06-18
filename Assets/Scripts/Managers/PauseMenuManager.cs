using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject panelPausaPrincipal;
    public GameObject panelOpciones;

    [Header("Ajustes de Sonido")]
    public Slider sliderMusica;
    public Slider sliderSFX;

    [Header("Configuración")]
    [Tooltip("Escribe el nombre exacto de la escena de tu menú principal (Ej: MainMenu)")]
    public string nombreMenuPrincipal = "MainMenu";

    private bool juegoPausado = false;

    private void Start()
    {
        // 🔥 ARREGLO 1: Forzamos a que el tiempo del juego corra normal al iniciar el nivel
        Time.timeScale = 1f;
        juegoPausado = false;

        // Nos aseguramos de que el menú de pausa esté apagado al empezar a jugar
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        // Conectamos los Sliders al MusicManager
        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(ActualizarVolumenMusica);
        }

        if (sliderSFX != null)
        {
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener(ActualizarVolumenSFX);
        }
    }

    private void Update()
    {
        bool presionaPausa = false;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            presionaPausa = true;
        }
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            presionaPausa = true;
        }

        if (presionaPausa)
        {
            if (juegoPausado) ReanudarJuego();
            else PausarJuego();
        }
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        Time.timeScale = 0f; // Congela el juego

        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // Descongela el juego

        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void AbrirOpciones()
    {
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);

            if (MusicManager.instance != null)
            {
                if (sliderMusica != null) sliderMusica.value = MusicManager.instance.audioSource.volume;
                if (sliderSFX != null) sliderSFX.value = MusicManager.instance.volumenSFXActual;
            }
        }
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(true);
    }

    public void SalirAlMenuPrincipal()
    {
        // Descongelamos el tiempo antes de salir
        Time.timeScale = 1f;

        // 🔥 Cargamos la escena del menú
        SceneManager.LoadScene(nombreMenuPrincipal);
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
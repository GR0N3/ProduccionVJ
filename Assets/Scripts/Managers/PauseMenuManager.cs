using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 🔥 Necesario para leer el teclado y mando

public class PauseMenuManager : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject panelPausaPrincipal;
    public GameObject panelOpciones;

    [Header("Ajustes de Sonido")]
    public Slider sliderMusica;
    public Slider sliderSFX;

    [Header("Configuración")]
    [Tooltip("Escribe el nombre exacto de la escena de tu menú principal")]
    public string nombreMenuPrincipal = "MainMenu"; // Cambiá esto por el nombre real de tu menú

    private bool juegoPausado = false;

    private void Start()
    {
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
        // Detectar la tecla ESC o el botón Options/Start del mando
        bool presionaPausa = false;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            presionaPausa = true;
        }
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            presionaPausa = true;
        }

        // Si se presionó el botón, alternamos la pausa
        if (presionaPausa)
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        Time.timeScale = 0f; // 🔥 CONGELA EL TIEMPO DEL JUEGO

        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // 🔥 DESCONGELA EL TIEMPO

        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void AbrirOpciones()
    {
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);

            // Sincronizamos las barritas con el volumen real al abrir
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
        // 🔥 ¡CRÍTICO! Si no descongelás el tiempo antes de salir, tu menú principal quedará trabado
        Time.timeScale = 1f;
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
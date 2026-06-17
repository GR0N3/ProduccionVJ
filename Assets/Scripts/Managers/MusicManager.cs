using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Música de Fondo")]
    public AudioSource audioSource;
    public AudioClip musicaFondo;
    [Range(0f, 1f)] public float volumenInicialMusica = 0.5f;

    [Header("Efectos de Sonido (SFX)")]
    [Range(0f, 1f)] public float volumenInicialSFX = 1f;
    [HideInInspector] public float volumenSFXActual; // El Player leerá este número

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Al arrancar, fijamos el volumen de los efectos
        volumenSFXActual = volumenInicialSFX;
    }

    private void Start()
    {
        if (audioSource != null && musicaFondo != null)
        {
            audioSource.clip = musicaFondo;
            audioSource.loop = true;
            audioSource.volume = volumenInicialMusica;
            audioSource.Play();
        }
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        if (audioSource != null) audioSource.volume = nuevoVolumen;
    }

    // Nueva función para el Slider de SFX
    public void CambiarVolumenSFX(float nuevoVolumen)
    {
        volumenSFXActual = nuevoVolumen;
    }
}
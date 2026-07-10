using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private PlayerControls controls; // reemplazá por el nombre real de tu clase generada

    public static event Action OnPause;
    public static event Action OnResume;

    private SessionController controller;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        controls = new PlayerControls();
        
    }

    private void Start()
    {
        ServiceLocator.Register<PauseManager>(this);
        controller = ServiceLocator.Get<SessionController>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.UI.Pause.performed += ctx => TogglePause(); // ajustá el nombre del Action Map/Action
    }

    private void OnDisable()
    {
        controls.UI.Pause.performed -= ctx => TogglePause();
        controls.Disable();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        OnPause?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        OnResume?.Invoke();
    }

    public void BackToMenu()
    {
        Resume();
        controller.BackToMainMenu();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PauseManager>();

        if (IsPaused)
            Time.timeScale = 1f;
    }
}
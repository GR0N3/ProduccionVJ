using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image timerBar;

    [SerializeField] private float maxTime = 60f;

    public float MaxTime => maxTime;

    private float remainingTime;

    public float RemainingTime => remainingTime;

    SessionController sessionController;

    private bool LevelBegin = false;

    private void OnEnable()
    {
        ShopController.OnLevelBegin += StartLevel;
    }

    private void OnDisable()
    {
        ShopController.OnLevelBegin -= StartLevel;
    }

    private void Start()
    {
        sessionController = ServiceLocator.Get<SessionController>();

        remainingTime = maxTime;

        timerBar.fillAmount = 1f;
    }

    private void Update()
    {
        if (LevelBegin) 
        {
            Countdown();
        }
    }

    public void Countdown()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime < 0)
            {
                remainingTime = 0;

                sessionController.BackToMainMenu();
            }

            timerBar.fillAmount = remainingTime / maxTime;
        }
    }

    public void StartLevel()
    {
        RestartTimer();
        LevelBegin = true;
    }

    public void RestartTimer()
    {
        LevelBegin = false;
        timerBar.fillAmount = 1f;
        remainingTime = maxTime;
    }


}
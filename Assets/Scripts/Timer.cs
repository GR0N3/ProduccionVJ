using UnityEngine;
using TMPro; // si usás TextMeshPro (recomendado)
public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
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
        UpdateTimerText();
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
            UpdateTimerText();
        }
    }
    private void UpdateTimerText()
    {
        int secondsToShow = Mathf.CeilToInt(remainingTime);
        timerText.text = secondsToShow.ToString();

        //Formato en mm:ss
        // int minutes = secondsToShow / 60;
        // int seconds = secondsToShow % 60;
        // timerText.text = $"{minutes:00}:{seconds:00}";
    }
    public void StartLevel()
    {
        RestartTimer();
        LevelBegin = true;
    }
    public void RestartTimer()
    {
        LevelBegin = false;
        remainingTime = maxTime;
        UpdateTimerText();
    }
}
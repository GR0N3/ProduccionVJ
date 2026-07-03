using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
public class SessionController : MonoBehaviour
{
    
    private int points = 0;

    private InputSystem_Actions inputActions;

    private int sceneIndex;

    [SerializeField] private List<string> sceneNames;

    [SerializeField] private TMP_Text pointsText; 

    [SerializeField] private Timer timer;
    [SerializeField] private float maxTimerBonus = 1000;
    public string CurrentScene { get; private set; }

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
        Door.OnLevelCompleted += LevelCompleted;
        PlayerHealth.OnPlayerDeath += ResetLevel;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
        Door.OnLevelCompleted -= LevelCompleted;
        PlayerHealth.OnPlayerDeath -= ResetLevel;
    }

    private string orignaltext;

    public PlayerManager PlayerManager;
    public float Points => points;

    private void Awake()
    {
        ServiceLocator.Register<SessionController>(this);
        sceneIndex = 0;
    }

    private void Start()
    {
        orignaltext = pointsText.text;

        pointsText.text += points.ToString();

        CurrentScene = sceneNames[sceneIndex];

    }

    private void ResetLevel()
    {
        sceneIndex = 0;
        CurrentScene = sceneNames[sceneIndex];
        BackToMainMenu();
    }

    private void LevelCompleted()
    {
        sceneIndex++;
        TimerPoints();

        timer.RestartTimer();

        if (sceneIndex > sceneNames.Count - 1)
        {

            ResetLevel();
        }
        else
        {
            CurrentScene = sceneNames[sceneIndex];
        }

        Debug.Log(sceneIndex);
    }

    public void UpdateScoreUI()
    {
        pointsText.text = orignaltext + (points).ToString();
    }

    public void BackToMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDataBase.Scenes.Match)
            .Unload(SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perfrom();
    }

    public void TimerPoints()
    {
        Debug.Log(timer.RemainingTime);

        float ratio =
            timer.RemainingTime /
            timer.MaxTime;

        int bonus =
            Mathf.RoundToInt(
                ratio *
                ratio *
                maxTimerBonus
            );

        AddPoints(bonus);
    }

    public void AddPoints(int amount)
    {
        points += amount;

        pointsText.text =
            orignaltext +
            points;
    }

    public void SubtractPoints(int amount)
    {
        points -= amount;
        pointsText.text =
            orignaltext +
            points;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<SessionController>();
    }

}

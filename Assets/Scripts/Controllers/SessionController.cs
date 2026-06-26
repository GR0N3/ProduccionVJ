using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
public class SessionController : MonoBehaviour
{
    
    private int points = 0;

    private InputSystem_Actions inputActions;

    private int sceneIndex;

    [SerializeField] private List<SceneAsset> sceneAssets;

    [SerializeField] private TMP_Text pointsText; 

    [SerializeField] private Timer timer;
    [SerializeField] private float maxTimerBonus;
    public SceneAsset CurrentScene {  get; private set; }

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

        CurrentScene = sceneAssets[sceneIndex];

    }

    private void ResetLevel()
    {
        sceneIndex = 0;
        CurrentScene = sceneAssets[sceneIndex];
        BackToMainMenu();
    }

    private void LevelCompleted()
    {
        sceneIndex++;

        if (sceneIndex > sceneAssets.Count - 1)
        {
            TimerPoints();

            timer.RestartTimer();

            ResetLevel();
        }
        else
        {
            CurrentScene = sceneAssets[sceneIndex];
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

        Debug.Log(bonus);

        AddPoints(bonus);
    }

    public void AddPoints(int amount)
    {
        points += amount;

        pointsText.text =
            orignaltext +
            points;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<SessionController>();
    }

}

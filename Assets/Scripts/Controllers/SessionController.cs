using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
public class SessionController : MonoBehaviour
{
    
    private int points = 0;

    private int sceneIndex;

    [SerializeField] private List<string> sceneNames;

    [SerializeField] private TMP_Text pointsText; 

    [SerializeField] private Timer timer;
    [SerializeField] private float maxTimerBonus = 1000;
    public string CurrentScene { get; private set; }

    public int SceneIndex => sceneIndex;

    public static event Action OnLevelBegin;

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
        Door.OnLevelCompleted += LevelCompleted;
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
        Door.OnLevelCompleted -= LevelCompleted;
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
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

        pointsText.text = orignaltext + points.ToString();

        UpdateCurrentScene();

        OnLevelBegin?.Invoke();

    }

    private void HandlePlayerDeath()
    {
        ResetSessionProgress();
        BackToMainMenu();
    }

    private void LevelCompleted()
    {
        sceneIndex++;
        TimerPoints();

        if (timer != null)
        {
            timer.RestartTimer();
        }

        if (sceneIndex > sceneNames.Count - 1)
        {
            ResetSessionProgress();
            BackToMainMenu();
        }
        else
        {
            UpdateCurrentScene();
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
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Unload(SceneDataBase.Slots.SessionContent)
            .Unload(SceneDataBase.Slots.Session)
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

        AudioManager.instance.Play("Coin");
    }



    public void SubtractPoints(int amount)
    {
        points -= amount;
        pointsText.text =
            orignaltext +
            points;
    }

    public void GoToMatch()
    {
        LoadCurrentLevel();
    }

    public void RestartCurrentLevel()
    {
        LoadCurrentLevel();
    }

    public void FinalizeCurrentLevel()
    {
        if (string.IsNullOrEmpty(CurrentScene))
        {
            return;
        }

        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.SessionContent, SceneDataBase.Scenes.Shop, setActive: true)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perfrom();
        AudioManager.instance.Play("Shop");
        LevelCompleted();
    }

    public void LoadLevelByIndex(int levelIndex)
    {
        if (!HasValidLevelIndex(levelIndex))
        {
            Debug.LogWarning($"Level index {levelIndex} fuera de rango.");
            return;
        }

        sceneIndex = levelIndex;
        UpdateCurrentScene();
        LoadCurrentLevel();
    }


    private void OnDestroy()
    {
        ServiceLocator.Unregister<SessionController>();
    }

    private void LoadCurrentLevel()
    {
        if (string.IsNullOrEmpty(CurrentScene))
        {
            return;
        }

        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.SessionContent, CurrentScene, setActive: true)
            .WithOverlay()
            .Perfrom();

        if (timer != null)
        {
            timer.RestartTimer();
        }

        RestorePlayerForLevel();
        OnLevelBegin?.Invoke();
    }

    private void RestorePlayerForLevel()
    {
        if (PlayerManager == null || PlayerManager.PlayerHealth == null)
        {
            return;
        }

        PlayerManager.PlayerHealth.GainHealth(PlayerManager.PlayerHealth.MaxHealth);
    }

    private void ResetSessionProgress()
    {
        sceneIndex = 0;
        UpdateCurrentScene();

        if (timer != null)
        {
            timer.RestartTimer();
        }
    }

    private void UpdateCurrentScene()
    {
        if (HasValidLevelIndex(sceneIndex))
        {
            CurrentScene = sceneNames[sceneIndex];
            return;
        }

        CurrentScene = string.Empty;
    }

    private bool HasValidLevelIndex(int levelIndex)
    {
        return levelIndex >= 0 && levelIndex < sceneNames.Count;
    }

}

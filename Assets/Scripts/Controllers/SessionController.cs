using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
[DefaultExecutionOrder(-99)]
public class SessionController : MonoBehaviour
{
    
    private float points = 0f;
    private int gold = 1;

    private InputSystem_Actions inputActions;

    private int sceneIndex;

    [SerializeField] private List<SceneAsset> sceneAssets;
    public SceneAsset CurrentScene {  get; private set; }

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
        Enemy.OnEnemyDeath += AddGold;
        Door.OnLevelCompleted += LevelCompleted;
        PlayerHealth.OnPlayerDeath += ResetLevel;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
        Enemy.OnEnemyDeath -= AddGold;
        Door.OnLevelCompleted -= LevelCompleted;
        PlayerHealth.OnPlayerDeath -= ResetLevel;
    }

    private string orignaltext;
    private string goldOriginalText;

    public PlayerManager PlayerManager;
    public float Points => points;
    public int Gold => gold;

    [SerializeField] private TMP_Text pointsText; 
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        ServiceLocator.Register<SessionController>(this);
        sceneIndex = 0;
    }

    private void Start()
    {
        orignaltext = pointsText.text;
        goldOriginalText = goldText.text;

        pointsText.text += points.ToString();
        goldText.text += gold.ToString();

        CurrentScene = sceneAssets[sceneIndex];

    }

    private void ResetLevel()
    {
        sceneIndex = 0;
        CurrentScene = sceneAssets[sceneIndex];
    }

    private void LevelCompleted()
    {
        sceneIndex++;
        if (sceneIndex > sceneAssets.Count - 1)
        {
            ResetLevel();
        }
        else
        {
            CurrentScene = sceneAssets[sceneIndex];
        }

        Debug.Log(sceneIndex);
    }

    private void AddGold()
    {
        gold += 1;
        UpdateGoldUI();
    }

    public void UpdateScoreUI()
    {
        points += 10;
        pointsText.text = orignaltext + (points).ToString();
        
    }
    public void UpdateGoldUI()
    {
        goldText.text = goldOriginalText + (gold).ToString();
    }

    public void ChangeGold(int result)
    {
        gold = result;
        UpdateGoldUI();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<SessionController>();
    }

}

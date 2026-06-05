using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
[DefaultExecutionOrder(-99)]
public class SessionController : MonoBehaviour
{
    
    private float points = 0f;
    private int gold = 0;

    private InputSystem_Actions inputActions;

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
        Enemy.OnEnemyDeath += AddGold;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
        Enemy.OnEnemyDeath -= AddGold;
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
    }

    private void Start()
    {
        orignaltext = pointsText.text;
        goldOriginalText = goldText.text;

        pointsText.text += points.ToString();
        goldText.text += gold.ToString();

    }

    public void UpdateScoreUI()
    {
        points += 10;
        pointsText.text = orignaltext + (points).ToString();
        
    }

    private void AddGold()
    {
        gold += 1;
        UpdateGoldUI();
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

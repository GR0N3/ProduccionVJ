using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
[DefaultExecutionOrder(-99)]
public class SessionController : MonoBehaviour
{
    public static SessionController Instance;
    
    private float points = 0f;
    private float gold = 9999f;

    private InputSystem_Actions inputActions;

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
    }

    private string orignaltext;

    public PlayerManager PlayerManager;
    public float Points => points;
    public float Gold => gold;

    [SerializeField] private TMP_Text pointsText; 
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

    }

    private void Start()
    {
        orignaltext = pointsText.text;

        pointsText.text += points.ToString();
        goldText.text += gold.ToString();

    }

    public void UpdateScoreUI()
    {
        points += 10;
        pointsText.text = orignaltext + (points).ToString();
        
    }

}

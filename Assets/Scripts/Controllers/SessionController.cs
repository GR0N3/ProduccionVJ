using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
[DefaultExecutionOrder(-99)]
public class SessionController : MonoBehaviour
{
    
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
        ServiceLocator.Register<SessionController>(this);
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

    private void OnDestroy()
    {
        ServiceLocator.Unregister<SessionController>();
    }

}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[DefaultExecutionOrder(-99)]
public class SessionController : MonoBehaviour
{
    public static SessionController Instance;

    private float points = 0f;
    private float gold = 9999f;

    private string orignaltext = "";

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
        // Chequeos de seguridad por si olvidaste asignar la UI en el Inspector
        if (pointsText != null)
        {
            orignaltext = pointsText.text;
            pointsText.text += points.ToString();
        }
        else
        {
            Debug.LogWarning("No asignaste el pointsText en el SessionController.");
        }

        if (goldText != null)
        {
            goldText.text += gold.ToString();
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateScoreUI;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateScoreUI;
    }

    public void UpdateScoreUI()
    {
        points += 10;
        if (pointsText != null)
        {
            pointsText.text = orignaltext + (points).ToString();
        }
    }
}
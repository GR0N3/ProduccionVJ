using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SessionController : MonoBehaviour
{
    public static SessionController Instance;
    
    private float points = 0f;
    private float gold = 9999f;

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
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        pointsText.text += points.ToString();
        goldText.text += gold.ToString();
    }


}

using UnityEngine;
using TMPro;

public class SessionController : MonoBehaviour
{
    private float points = 0;
    private float gold = 9999f;

    public float Points => points;
    public float Gold => gold;

    [SerializeField] private TMP_Text pointsText; 
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        pointsText.text += points.ToString();
        goldText.text += gold.ToString();
    }


    
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script gestiona el puntaje y ahora puede crear automáticamente la interfaz si no existe.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Configuración de UI")]
    // Si dejas esto vacío, el script intentará crear un Canvas y un Texto automáticamente.
    public TextMeshProUGUI scoreText;

    private int score = 0;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Si no asignaste un texto en el Inspector, intentamos crearlo por código.
        if (scoreText == null)
        {
            CreateDefaultScoreUI();
        }
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    // Método para sumar puntos
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("<color=green>Puntaje sumado: " + points + ". Total: " + score + "</color>");
        UpdateScoreUI();
    }

    // Actualiza el contenido del texto
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntaje: " + score;
        }
    }

    // Crea un Canvas y un objeto de Texto automáticamente en la parte superior central.
    private void CreateDefaultScoreUI()
    {
        // 1. Crear el Canvas
        GameObject canvasObj = new GameObject("ScoreCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Crear el objeto de Texto (TextMeshPro)
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvasObj.transform);

        scoreText = textObj.AddComponent<TextMeshProUGUI>();
        
        // 3. Configurar posición (Superior Central)
        RectTransform rectTransform = scoreText.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 1f); // Centro arriba
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0, -50); // 50 pixeles abajo del borde
        rectTransform.sizeDelta = new Vector2(300, 50);

        // 4. Configurar estilo del texto
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        
        Debug.Log("ScoreManager: Se ha creado automáticamente una interfaz de puntaje.");
    }

    public int GetScore()
    {
        return score;
    }
}

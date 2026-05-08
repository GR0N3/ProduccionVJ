using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script gestiona el puntaje y ahora puede crear automáticamente la interfaz si no existe.
public class ScoreManager : MonoBehaviour
{
    private int score = 0;

    //private void OnEnable()
    //{
    //    Enemy.OnEnemyDeath += UpdateScoreUI;
    //}

    //private void OnDisable()
    //{
    //    Enemy.OnEnemyDeath -= UpdateScoreUI;
    //}

    private void Awake()
    {

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
        
    }
    public int GetScore()
    {
        return score;
    }
}

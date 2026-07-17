using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;


    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void OnEnable()
    {
        SceneController.OnSceneChanged += ChangeMusic;
    }


    private void OnDisable()
    {
        SceneController.OnSceneChanged -= ChangeMusic;
    }


    private void ChangeMusic()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainMenu":
                AudioManager.instance.Play("MainMenu");
                break;

            case "Shop":
                AudioManager.instance.Play("Shop");

                break;

            case "Session":
                AudioManager.instance.Play("Gameplay");
                break;
        }
    }
}
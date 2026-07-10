using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    private void Awake()
    {
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void StartSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Session, SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.SessionContent, "LVL1Rework A")
            .Unload(SceneDataBase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perfrom();
    }
}

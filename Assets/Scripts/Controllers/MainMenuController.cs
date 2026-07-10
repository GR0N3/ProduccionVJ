using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    private void Awake()
    {
    }


    public void StartSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Session, SceneDataBase.Scenes.Session, setActive :true)
            .Load(SceneDataBase.Slots.SessionContent, "LVL1Rework A")
            .Unload(SceneDataBase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perfrom();
    }
}

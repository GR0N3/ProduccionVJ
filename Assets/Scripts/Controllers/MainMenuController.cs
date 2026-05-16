using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void StartSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Session, SceneDataBase.Scenes.Session, setActive :true)
            .Load(SceneDataBase.Slots.SessionContent, SceneDataBase.Scenes.Shop)
            .Unload(SceneDataBase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perfrom();
    }
}

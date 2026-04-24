using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Session, SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.SessionContent, SceneDataBase.Scenes.Shop, setActive: true)
            .Unload(SceneDataBase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perfrom();
    }
}

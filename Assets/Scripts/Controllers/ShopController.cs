using UnityEngine;

public class ShopController : MonoBehaviour
{
    private SessionController sessionController;
    private void Awake()
    {
        sessionController = ServiceLocator.Get<SessionController>();
    }

    public void GoToMatch()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.SessionContent, sessionController.CurrentScene.name)
            .WithOverlay()
            .Unload(SceneDataBase.Scenes.Shop)
            .Perfrom();
    }
}

using UnityEngine;

public class ShopController : MonoBehaviour
{
    public void GoToMatch()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.SessionContent, SceneDataBase.Scenes.Match)
            .WithOverlay()
            .Unload(SceneDataBase.Scenes.Shop)
            .Perfrom();
    }
}

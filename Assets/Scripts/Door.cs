using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneController.Instance
                .NewTransition()
                .Load(SceneDataBase.Slots.SessionContent, SceneDataBase.Scenes.Shop, setActive: true)
                .WithClearUnusedAssets()
                .WithOverlay()
                .Unload(SceneDataBase.Slots.SessionContent)
                .Perfrom();
        }
    }
}

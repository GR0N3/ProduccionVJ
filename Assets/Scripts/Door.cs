using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static event Action OnLevelCompleted;

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

            OnLevelCompleted?.Invoke();

        }
    }
}

using UnityEngine;

public class DeathWall : MonoBehaviour
{
    public Transform player;

    [Tooltip("Distancia detrás del jugador")]
    public float followOffset = 8f;

    public bool neverMoveBack = true;

    float maxX;

    void Start()
    {
        maxX = transform.position.x;
    }

    void LateUpdate()
    {
        float targetX = player.position.x - followOffset;

        if (neverMoveBack)
        {
            if (targetX > maxX)
                maxX = targetX;

            targetX = maxX;
        }

        // Y se mantiene fija
        transform.position = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneController.Instance
                .NewTransition()
                .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
                .WithClearUnusedAssets()
                .WithOverlay()
                .Unload(SceneDataBase.Slots.SessionContent)
                .Unload(SceneDataBase.Slots.Session)
                .Perfrom();
        }
    }
}
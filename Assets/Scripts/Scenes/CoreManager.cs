using UnityEngine;

public class CoreManager : MonoBehaviour
{

    void Start()
    {
        //Sistema Core para el setup
        //Cargar todo lo necesario (managers, saves, datos, audio)

        //se llama al controlador de escenas para cargar lo necesario.
        //primero NewTransition y ultimo Perform.
        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithOverlay()
            .Perfrom();
    }
}

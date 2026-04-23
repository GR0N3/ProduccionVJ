using UnityEngine;

public class CoreManager : MonoBehaviour
{

    void Start()
    {
        //Sistema Core para el setup
        //Cargar todo lo necesario (managers, saves, datos, audio)

        //se llama al controlador de escenas para cargar lo necesario.
        //primero NewTransition y ultimo Perform.
        //Cargar SessionContent sirve para escenas con datos temporales
        //Session se deja cargada la majoria de las veces pq ahi guardamos los datos constantes de la partida

        SceneController.Instance
            .NewTransition()
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .Perfrom();
    }
}

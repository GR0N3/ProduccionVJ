using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerHealth playerHealth = new();
    private PlayerMovement playerMovement;
    private PlayerWeapon playerWeapon;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerWeapon PlayerWeapon => playerWeapon;

    private void Awake()
    {
        SessionController.Instance.PlayerManager = this;
    }

    //considerar un constructor para las clases del player en caso de querer eliminar monobehaviours
    //player manager deberia ir en player y el player en session?

    //Usar el manager para las mejoras 


}

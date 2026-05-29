using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int maxHealthPoints = 10;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerWeapon playerWeapon;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerWeapon PlayerWeapon => playerWeapon;
    public int MaxHealthPoints => maxHealthPoints;

    private void Awake()
    {
        SessionController.Instance.PlayerManager = this;

        playerWeapon = new PlayerWeapon();
        playerHealth = new PlayerHealth();
        playerMovement = new PlayerMovement();

        playerHealth.Init(maxHealthPoints);
    }

    //considerar un constructor para las clases del player en caso de querer eliminar monobehaviours
    //player manager deberia ir en player y/o el player en session?

    //Usar el manager para las mejoras 


}

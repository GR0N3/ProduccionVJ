using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private SessionController sessionController;

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
        sessionController = ServiceLocator.Get<SessionController>();

        sessionController.PlayerManager = this;

        ServiceLocator.Register<PlayerManager>(this);

        playerWeapon = new PlayerWeapon();
        playerHealth = new PlayerHealth();
        playerMovement = new PlayerMovement();

        playerHealth.Init(maxHealthPoints);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerManager>();
    }

    //Usar el manager para las mejoras 

}

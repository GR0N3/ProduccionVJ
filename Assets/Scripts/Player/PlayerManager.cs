using UnityEngine;

[DefaultExecutionOrder(-98)]
public class PlayerManager : MonoBehaviour
{
    private SessionController sessionController;

    private int maxHealthPoints = 10;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerWeapon playerWeapon;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerWeapon PlayerWeapon => playerWeapon;
    public int MaxHealthPoints => maxHealthPoints;

    public Stats Stats { get; private set; }

    private void Awake()
    {
        sessionController = ServiceLocator.Get<SessionController>();

        sessionController.PlayerManager = this;

        ServiceLocator.Register<PlayerManager>(this);

        Stats = new Stats();

        playerWeapon = new PlayerWeapon();
        playerHealth = new PlayerHealth();
        playerMovement = new PlayerMovement();

        InitialStats();
    }

    private void InitialStats() 
    {
        Stats.SetStat(UpgradeType.MaxHealth, 10);
        Stats.SetStat(UpgradeType.Speed, 10);
        Stats.SetStat(UpgradeType.JumpForce, 20);
        Stats.SetStat(UpgradeType.Acceleration, 20);
        Stats.SetStat(UpgradeType.Deceleration, 25);
        Stats.SetStat(UpgradeType.Damage, 1);
        Stats.SetStat(UpgradeType.BulletSpeed, 10);
        Stats.SetStat(UpgradeType.BullesCount, 10);
        Stats.SetStat(UpgradeType.BulletsSpread, 0);
        Stats.SetStat(UpgradeType.KnockbackForce, 0.5f);
        Stats.SetStat(UpgradeType.Acceleration, 20);
        Stats.SetStat(UpgradeType.Deceleration, 25);

        playerHealth.Init((int)Stats.GetStat(UpgradeType.MaxHealth));
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerManager>();
    }

    //Usar el manager para las mejoras 

}

using UnityEngine;

[DefaultExecutionOrder(-90)]
public class PlayerManager : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerWeapon playerWeapon;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerWeapon PlayerWeapon => playerWeapon;
    public Stats Stats { get; private set; }

    [SerializeField] private float maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float riseMultiplier = 1.5f;
    [SerializeField] private float maxJumpSpeed = 20f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float damage;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletsCount;
    [SerializeField] private float bulletsSpread;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float knockbackForce;
    [SerializeField] private bool startWithGodMode;

    public float RiseMultiplier => riseMultiplier;
    public float MaxJumpSpeed => maxJumpSpeed;
    public float FallMultiplier => fallMultiplier;
    public float MaxFallSpeed => maxFallSpeed;
    public bool IsGodMode { get; private set; }

    private void Awake()
    {
        Debug.Log("PlayerManager Awake");
        ServiceLocator.Register<PlayerManager>(this);

        Stats = new Stats();

        playerWeapon = new PlayerWeapon();
        playerHealth = new PlayerHealth();
        playerMovement = new PlayerMovement();

        InitialStats();
        IsGodMode = startWithGodMode;
    }

    private void InitialStats() 
    {
        Stats.SetStat(UpgradeType.MaxHealth, maxHealth);
        Stats.SetStat(UpgradeType.Speed, speed);
        Stats.SetStat(UpgradeType.JumpForce, jumpForce);
        Stats.SetStat(UpgradeType.Acceleration, acceleration);
        Stats.SetStat(UpgradeType.Deceleration, deceleration);
        Stats.SetStat(UpgradeType.Damage, damage);
        Stats.SetStat(UpgradeType.BulletSpeed, bulletSpeed);
        Stats.SetStat(UpgradeType.BulletsCount, bulletsCount);
        Stats.SetStat(UpgradeType.BulletsSpread,bulletsSpread);
        Stats.SetStat(UpgradeType.AttackSpeed, attackSpeed);
        Stats.SetStat(UpgradeType.KnockbackForce, knockbackForce);

        playerHealth.SetHealth((int)Stats.GetStat(UpgradeType.MaxHealth));
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerManager>();
    }

    public void SetGodMode(bool enabled)
    {
        IsGodMode = enabled;
    }

    public void ToggleGodMode()
    {
        SetGodMode(!IsGodMode);
    }

}

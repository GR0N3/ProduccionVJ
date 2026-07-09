using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private PlayerManager playerManager;

    private PlayerHealth health;
    public int maxHealth => (int)playerManager.Stats.GetStat(UpgradeType.MaxHealth);

    private PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    public int Damage => (int)playerManager.Stats.GetStat(UpgradeType.Damage);
    public int BulletsCount => (int)playerManager.Stats.GetStat(UpgradeType.BulletsCount);
    public float BulletSpread => playerManager.Stats.GetStat(UpgradeType.BulletsSpread);
    public float BulletSpeed => playerManager.Stats.GetStat(UpgradeType.BulletSpeed);
    public float KnockbackForce => playerManager.Stats.GetStat(UpgradeType.KnockbackForce);
    public float AttackSpeed => playerManager.Stats.GetStat(UpgradeType.AttackSpeed);

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    private PlayerMovement movement;
    public Rigidbody2D rb;
    public Collider2D col;
    public float Speed => playerManager.Stats.GetStat(UpgradeType.Speed);
    public float JumpForce => playerManager.Stats.GetStat(UpgradeType.JumpForce);
    public float Acceleration => playerManager.Stats.GetStat(UpgradeType.Acceleration);
    public float Deceleration => playerManager.Stats.GetStat(UpgradeType.Deceleration);

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask borderLayer;

    public LayerMask GroundLayer => groundLayer;
    public LayerMask BorderLayer => borderLayer;

    public InputSystem_Actions InputActions { get; private set; }

    public Animator Animator { get; private set; }
    public AnimatorBrain AnimatorBrain { get; private set; }
    private void Awake()
    {
        playerManager = ServiceLocator.Get<PlayerManager>();

        InputActions = new();

        weapon = playerManager.PlayerWeapon;
        health = playerManager.PlayerHealth;
        movement = playerManager.PlayerMovement;

        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();

        AnimatorBrain = new();
        AnimatorBrain.SetIdle(PlayerAnimations.Idle);
        AnimatorBrain.Init(Animator);

        weapon.Init(this);
        movement.Init(this);
        health.Init(this);
    }

    private void OnEnable()
    {
        InputActions.Enable();

        InputActions.Player.Attack.performed += weapon.OnFire;
        InputActions.Player.Move.performed += weapon.OnMove;

        InputActions.Player.Move.performed += movement.OnMove;
        InputActions.Player.Move.canceled += movement.OnMove;
        InputActions.Player.Jump.performed += movement.OnJump;
    }

    private void OnDisable()
    {
        InputActions.Player.Attack.performed -= weapon.OnFire;
        InputActions.Player.Move.performed -= weapon.OnMove;

        InputActions.Player.Move.performed -= movement.OnMove;
        InputActions.Player.Move.canceled -= movement.OnMove;
        InputActions.Player.Jump.performed -= movement.OnJump;

        InputActions.Disable();
    }

    private void Update()
    {
        movement.Tick();
        AnimatorBrain.Tick();
    }

    private void FixedUpdate()
    {
        movement.FixedTick();
    }

    public void Shoot()
    {
        weapon.Shoot();
    }

    public void TakeDamage(int damage, Vector2 direction, float knockback)
    {
        health.TakeDamage(damage, direction, knockback);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.layer == 8))
        {
            TakeDamage(1, new Vector2(-1, -1), 25f);
            Debug.Log("damage for collision");
        }
    }
}
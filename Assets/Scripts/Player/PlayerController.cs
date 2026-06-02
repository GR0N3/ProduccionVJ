using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Health")]
    PlayerHealth health;
    public int maxHealth;

    [Header("Weapon")]
    PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    int damage = 1;
    public int Damage => damage;
    int bulletsCount = 1;
    public int BulletsCount => bulletsCount;
    float bulletsSpread = 0f;
    public float BulletSpread => bulletsSpread;
    float bulletSpeed = 10f;
    public float BulletSpeed => bulletSpeed;
    float knockbackForce = 10;
    public float KnockbackForce => knockbackForce;

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [Header("Movement")]
    private PlayerMovement movement;
    public Rigidbody2D rb;
    [SerializeField] public float speed;
    [SerializeField] public float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask borderLayer;
    public LayerMask GroundLayer => groundLayer;
    public LayerMask BorderLayer => borderLayer;

    [Header(" - Border")]

    private InputSystem_Actions inputActions;
    public InputSystem_Actions InputActions => inputActions;

    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = ServiceLocator.Get<PlayerManager>();

        inputActions = new();
        weapon = playerManager.PlayerWeapon;
        health = playerManager.PlayerHealth;
        movement = playerManager.PlayerMovement;
        rb = GetComponent<Rigidbody2D>();
        weapon.Init(this);
        movement.Init(this);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        
        inputActions.Player.Attack.performed += weapon.OnFire;           
        inputActions.Player.Move.performed += weapon.OnMove;             
        inputActions.Player.AltAttack.performed += weapon.OnAltFire;

        inputActions.Player.Move.performed += movement.OnMove;
        inputActions.Player.Move.canceled += movement.OnMove;
        inputActions.Player.Jump.performed += movement.OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= weapon.OnFire;
        inputActions.Player.Move.performed -= weapon.OnMove;
        inputActions.Player.AltAttack.performed -= weapon.OnAltFire;

        inputActions.Player.Move.performed -= movement.OnMove;
        inputActions.Player.Move.canceled -= movement.OnMove; 
        inputActions.Player.Jump.performed -= movement.OnJump;

        inputActions.Disable();
    }

    private void Update()
    {
        movement.Tick();
    }

    private void FixedUpdate()
    {
        movement.FixedTick();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.layer == 8))
        {
            health.TakeDamage(1, new Vector2(-1, -1), 25f);
        }
    }
}

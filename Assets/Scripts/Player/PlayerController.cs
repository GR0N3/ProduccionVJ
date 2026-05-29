using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Health")]
    PlayerHealth health;
    public int maxHealth;
    int currentHealth;

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
    float knockbackForce;
    public float KnockbackForce => knockbackForce;
    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [Header("Movement")]
    PlayerMovement movement;
    float speed;
    float jumpForce;
    public Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    public LayerMask GroundLayer => groundLayer;
    [Header(" - Border")]
    [SerializeField] private Transform leftBorder;
    public Transform LeftBorder => leftBorder;

    private InputSystem_Actions inputActions;
    public InputSystem_Actions InputActions => inputActions;

    private void Awake()
    {
        var DataRef = SessionController.Instance.PlayerManager;
        inputActions = new();
        weapon = DataRef.PlayerWeapon;
        health = DataRef.PlayerHealth;
        movement = DataRef.PlayerMovement;
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
        if ((8 & (1 << collision.gameObject.layer)) != 0)
        {
            health.TakeDamage(1, new Vector2(-1, -1), 25f);
        }
    }



}

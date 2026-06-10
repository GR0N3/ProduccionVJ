using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    PlayerHealth health;
    public int maxHealth = 10;

    [Header("Weapon Settings")]
    PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;

    [SerializeField] private float bulletLifetime = 1.5f;
    public float BulletLifetime => bulletLifetime;

    [SerializeField] private float bulletSpeed = 15f;
    public float BulletSpeed => bulletSpeed;

    [SerializeField] private int damage = 1;
    public int Damage => damage;

    [SerializeField] private int bulletsCount = 1;
    public int BulletsCount => bulletsCount;

    [SerializeField] private float bulletsSpread = 15f;
    public float BulletSpread => bulletsSpread;

    [SerializeField] private float knockbackForce = 5f;
    public float KnockbackForce => knockbackForce;

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [Header("Movement Settings")]
    private PlayerMovement movement;
    public Rigidbody2D rb;

    // NUEVO: Variable para controlar las animaciones
    public Animator animator;

    [SerializeField] public float speed = 15f;
    [SerializeField] public float jumpForce = 12f;
    [SerializeField] public float jumpCutMultiplier = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask borderLayer;
    public LayerMask GroundLayer => groundLayer;
    public LayerMask BorderLayer => borderLayer;

    private InputSystem_Actions inputActions;
    public InputSystem_Actions InputActions => inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();

        // NUEVO: Agarramos el Animator de tu personaje
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (SessionController.Instance != null && SessionController.Instance.PlayerManager != null)
        {
            var DataRef = SessionController.Instance.PlayerManager;

            weapon = DataRef.PlayerWeapon;
            health = DataRef.PlayerHealth;
            movement = DataRef.PlayerMovement;

            if (weapon != null) weapon.Init(this);
            if (movement != null) movement.Init(this);

            inputActions.Player.Attack.performed += weapon.OnFire;
            inputActions.Player.Move.performed += weapon.OnMove;
            inputActions.Player.AltAttack.performed += weapon.OnAltFire;

            inputActions.Player.Move.performed += movement.OnMove;
            inputActions.Player.Move.canceled += movement.OnMove;

            inputActions.Player.Jump.performed += movement.OnJump;
            inputActions.Player.Jump.canceled += movement.OnJumpCanceled;
        }
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null && weapon != null && movement != null)
        {
            inputActions.Player.Attack.performed -= weapon.OnFire;
            inputActions.Player.Move.performed -= weapon.OnMove;
            inputActions.Player.AltAttack.performed -= weapon.OnAltFire;

            inputActions.Player.Move.performed -= movement.OnMove;
            inputActions.Player.Move.canceled -= movement.OnMove;

            inputActions.Player.Jump.performed -= movement.OnJump;
            inputActions.Player.Jump.canceled -= movement.OnJumpCanceled;

            inputActions.Disable();
        }
    }

    private void Update()
    {
        if (movement != null) movement.Tick();
    }

    private void FixedUpdate()
    {
        if (movement != null) movement.FixedTick();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.layer == 8))
        {
            if (health != null)
            {
                health.TakeDamage(1, new Vector2(-1, -1), 25f);
                if (health.CurrentHealth > 0) movement.RespawnAtSafePosition();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.layer == 8))
        {
            if (health != null)
            {
                health.TakeDamage(1, new Vector2(-1, -1), 25f);
                if (health.CurrentHealth > 0) movement.RespawnAtSafePosition();
            }
        }
    }
}
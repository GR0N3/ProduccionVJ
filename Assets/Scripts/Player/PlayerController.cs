using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Health")]
    private PlayerHealth health;
    public int maxHealth;

    [Header("Weapon")]
    private PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    public int Damage { get; private set; }
    public int BulletsCount { get; private set; }
    public float BulletSpread { get; private set; }
    public float BulletSpeed { get; private set; }
    public float KnockbackForce { get; private set; }

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [Header("Movement")]
    private PlayerMovement movement;
    public Rigidbody2D rb;
    public float Speed { get; private set; }
    public float JumpForce { get; private set; }
    public LayerMask GroundLayer { get; private set; }
    public LayerMask BorderLayer { get; private set; }

    [Header(" - Border")]
    public InputSystem_Actions InputActions { get; private set; }

    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = ServiceLocator.Get<PlayerManager>();

        InputActions = new();
        weapon = playerManager.PlayerWeapon;
        health = playerManager.PlayerHealth;
        movement = playerManager.PlayerMovement;
        rb = GetComponent<Rigidbody2D>();
        weapon.Init(this);
        movement.Init(this);
    }

    private void OnEnable()
    {
        InputActions.Enable();
        
        InputActions.Player.Attack.performed += weapon.OnFire;           
        InputActions.Player.Move.performed += weapon.OnMove;             
        InputActions.Player.AltAttack.performed += weapon.OnAltFire;
            
        InputActions.Player.Move.performed += movement.OnMove;
        InputActions.Player.Move.canceled += movement.OnMove;
        InputActions.Player.Jump.performed += movement.OnJump;
    }

    private void OnDisable()
    {
        InputActions.Player.Attack.performed -= weapon.OnFire;
        InputActions.Player.Move.performed -= weapon.OnMove;
        InputActions.Player.AltAttack.performed -= weapon.OnAltFire;

        InputActions.Player.Move.performed -= movement.OnMove;
        InputActions.Player.Move.canceled -= movement.OnMove; 
        InputActions.Player.Jump.performed -= movement.OnJump;

        InputActions.Disable();
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

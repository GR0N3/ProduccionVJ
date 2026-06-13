using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Salud y Reaparición")]
    PlayerHealth health;
    public int maxHealth = 10;
    public int currentHealth;

    public Vector2 ultimoCheckpoint;

    [Header("I-Frames (Inmortalidad)")]
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    [Header("Game Feel")]
    [Tooltip("El 'changüí' en segundos que te deja saltar tras salir del borde")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Weapon Settings")]
    PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;

    [SerializeField] private float fireCooldown = 0.5f;
    public float FireCooldown => fireCooldown;

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

    public Animator animator;

    [SerializeField] public float speed = 15f;
    [SerializeField] public float jumpForce = 12f;
    [SerializeField] public float jumpCutMultiplier = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask borderLayer;
    public LayerMask GroundLayer => groundLayer;
    public LayerMask BorderLayer => borderLayer;

    [Header("Sensores (Raycasts)")]
    public Transform controladorSuelo;
    public Transform controladorPared;
    public float distanciaSuelo = 0.2f;
    public float distanciaPared = 0.2f;
    public LayerMask capaSuelo;

    public bool enSuelo;
    public bool tocandoPared;

    private InputSystem_Actions inputActions;
    public InputSystem_Actions InputActions => inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        ultimoCheckpoint = transform.position;

        if (SessionController.Instance != null && SessionController.Instance.PlayerManager != null)
        {
            var DataRef = SessionController.Instance.PlayerManager;

            weapon = DataRef.PlayerWeapon;
            health = DataRef.PlayerHealth;
            movement = DataRef.PlayerMovement;

            if (health != null) health.Init(maxHealth);
            if (weapon != null) weapon.Init(this);
            if (movement != null) movement.Init(this);

            inputActions.Player.Attack.performed += weapon.OnFire;
            inputActions.Player.Move.performed += weapon.OnMove;
            inputActions.Player.AltAttack.performed += weapon.OnAltFire;

            inputActions.Player.Move.performed += movement.OnMove;
            inputActions.Player.Move.canceled += movement.OnMove;

            // Reemplazamos el botón de salto por nuestras nuevas funciones
            inputActions.Player.Jump.performed += IntentarSalto;
            inputActions.Player.Jump.canceled += CancelarSalto;
        }
    }

    private void OnEnable() { if (inputActions != null) inputActions.Enable(); }

    private void OnDisable()
    {
        if (inputActions != null && weapon != null && movement != null)
        {
            inputActions.Player.Attack.performed -= weapon.OnFire;
            inputActions.Player.Move.performed -= weapon.OnMove;
            inputActions.Player.AltAttack.performed -= weapon.OnAltFire;
            inputActions.Player.Move.performed -= movement.OnMove;
            inputActions.Player.Move.canceled -= movement.OnMove;

            inputActions.Player.Jump.performed -= IntentarSalto;
            inputActions.Player.Jump.canceled -= CancelarSalto;

            inputActions.Disable();
        }
    }

    private void Update()
    {
        // Permitimos que el script viejo siga manejando el caminar (izq/der)
        if (movement != null) movement.Tick();

        // --- SENSORES ---
        if (controladorSuelo != null)
        {
            enSuelo = Physics2D.Raycast(controladorSuelo.position, Vector2.down, distanciaSuelo, capaSuelo);
            Debug.DrawRay(controladorSuelo.position, Vector2.down * distanciaSuelo, Color.green);
        }

        if (controladorPared != null)
        {
            float direccionX = transform.localScale.x > 0 ? 1 : -1;
            Vector2 direccionMirada = new Vector2(direccionX, 0);

            tocandoPared = Physics2D.Raycast(controladorPared.position, direccionMirada, distanciaPared, capaSuelo);
            Debug.DrawRay(controladorPared.position, direccionMirada * distanciaPared, Color.red);
        }

        // --- LA MATEMÁTICA DEL COYOTE TIME ---
        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // --- I-FRAMES ---
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            if (spriteRenderer != null)
            {
                float alpha = (Mathf.Sin(Time.time * 35f) * 0.5f) + 0.5f;
                spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            }

            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
            }
        }
    }

    private void FixedUpdate() { if (movement != null) movement.FixedTick(); }

    // --- EL NUEVO SALTO 100% CONTROLADO POR NOSOTROS ---
    private void IntentarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Si el tiempo de Coyote aún no se agotó...
        if (coyoteTimeCounter > 0f)
        {
            // ¡IGNORAMOS AL SCRIPT VIEJO Y LO HACEMOS SALTAR NOSOTROS!
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Vaciamos el contador para que no haga doble salto en el aire
            coyoteTimeCounter = 0f;
        }
    }

    private void CancelarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Si el jugador suelta la barra espaciadora rápido, le cortamos el envión.
        // Esto crea el "Salto Variable" (si tocás cortito, salta bajito).
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    // --- SISTEMA DE DAÑO Y CHECKPOINTS ---
    public void TakeDamage(int damageAmount, Vector2 knockbackDir, float knockbackForceValue)
    {
        if (isInvincible) return;
        currentHealth -= damageAmount;

        if (currentHealth <= 0) Revivir();
        else
        {
            if (health != null) health.TakeDamage(damageAmount, knockbackDir, knockbackForceValue);

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForceValue, ForceMode2D.Impulse);
        }
    }

    public void Revivir()
    {
        currentHealth = maxHealth;
        if (health != null) health.Init(maxHealth);

        transform.position = ultimoCheckpoint;
        rb.linearVelocity = Vector2.zero;

        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
    }

    private void ProcesarChoques(GameObject objetoTocado)
    {
        if (movement == null) return;
        string objName = objetoTocado.name.ToLower();
        string objTag = objetoTocado.tag.ToLower();

        if (objetoTocado.layer == 11)
        {
            currentHealth -= 1;
            if (currentHealth <= 0) Revivir();
            else
            {
                if (health != null) health.TakeDamage(1, Vector2.zero, 0f);
                transform.position = ultimoCheckpoint;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (objName.Contains("checkpoint") || objTag.Contains("checkpoint"))
        {
            ultimoCheckpoint = objetoTocado.transform.position;
            movement.SetRespawnPoint(objetoTocado.transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) { ProcesarChoques(collision.gameObject); }
    private void OnTriggerEnter2D(Collider2D collision) { ProcesarChoques(collision.gameObject); }
}
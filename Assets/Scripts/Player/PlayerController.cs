using UnityEngine;
using UnityEngine.UI;

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
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Sistema de Estamina y Escalada")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainHold = 10f;
    public float staminaDrainClimb = 20f;
    public float staminaJumpCost = 30f;
    public float climbSpeed = 5f;

    [Tooltip("Tiempo de gracia para no volver a pegarse instantáneamente al saltar de la pared")]
    public float wallJumpCooldown = 0.2f;
    private float wallJumpTimer;

    [Range(0f, 1f)]
    public float umbralEstaminaBaja = 0.25f;
    public Slider barraEstamina;

    private bool isGrabbingWall = false;
    private bool canGrab = true;
    private float originalGravityScale;

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
        originalGravityScale = rb.gravityScale;
        currentStamina = maxStamina;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue = maxStamina;
            barraEstamina.value = maxStamina;
            barraEstamina.gameObject.SetActive(false);
        }

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
        if (movement != null && !isGrabbingWall) movement.Tick();

        if (controladorSuelo != null)
        {
            enSuelo = Physics2D.Raycast(controladorSuelo.position, Vector2.down, distanciaSuelo, capaSuelo);
            Debug.DrawRay(controladorSuelo.position, Vector2.down * distanciaSuelo, Color.green);
        }

        if (controladorPared != null)
        {
            float direccionMiradaX = transform.localScale.x > 0 ? 1 : -1;
            Vector2 direccionMirada = new Vector2(direccionMiradaX, 0);

            tocandoPared = Physics2D.Raycast(controladorPared.position, direccionMirada, distanciaPared, capaSuelo);
            Debug.DrawRay(controladorPared.position, direccionMirada * distanciaPared, Color.red);
        }

        bool presionaGanchos = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            presionaGanchos = UnityEngine.InputSystem.Keyboard.current.eKey.isPressed;
        }

        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime; // Descuenta el tiempo anti-imán

        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, 60f * Time.deltaTime);
            canGrab = true;

            if (barraEstamina != null && barraEstamina.gameObject.activeSelf && currentStamina >= maxStamina)
            {
                barraEstamina.gameObject.SetActive(false);
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // VALIDACIÓN: Solo podés agarrarte si tenés estamina Y el timer de salto está en 0
        if (tocandoPared && !enSuelo && presionaGanchos && canGrab && currentStamina > 0 && wallJumpTimer <= 0)
        {
            if (!isGrabbingWall)
            {
                isGrabbingWall = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            if (isGrabbingWall) DesactivarAgarre();
        }

        if (isGrabbingWall)
        {
            if (barraEstamina != null && !barraEstamina.gameObject.activeSelf)
                barraEstamina.gameObject.SetActive(true);

            Vector2 inputsMovimiento = inputActions.Player.Move.ReadValue<Vector2>();

            float tasaDrenaje = (inputsMovimiento.y != 0) ? staminaDrainClimb : staminaDrainHold;
            currentStamina -= tasaDrenaje * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canGrab = false;
                DesactivarAgarre();
            }

            ProcesarFeedbackVisual();
        }

        if (barraEstamina != null && barraEstamina.gameObject.activeSelf)
        {
            barraEstamina.value = currentStamina;
        }

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (spriteRenderer != null && !isGrabbingWall)
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

    private void FixedUpdate()
    {
        if (isGrabbingWall)
        {
            Vector2 inputsMovimiento = inputActions.Player.Move.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(0f, inputsMovimiento.y * climbSpeed);
        }
        else
        {
            if (movement != null) movement.FixedTick();
        }
    }

    private void DesactivarAgarre()
    {
        isGrabbingWall = false;
        rb.gravityScale = originalGravityScale;
        if (spriteRenderer != null && !isInvincible) spriteRenderer.color = colorOriginal;
    }

    private void ProcesarFeedbackVisual()
    {
        float ratioEstamina = currentStamina / maxStamina;

        if (barraEstamina != null)
        {
            Image fillImage = barraEstamina.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (ratioEstamina <= umbralEstaminaBaja)
                {
                    float frecuenciaParpadeo = Mathf.PingPong(Time.time * 15f, 1f);
                    fillImage.color = Color.Lerp(Color.red, new Color(0.3f, 0f, 0f), frecuenciaParpadeo);

                    if (spriteRenderer != null)
                        spriteRenderer.color = Color.Lerp(colorOriginal, Color.red, 0.4f);
                }
                else
                {
                    fillImage.color = Color.green;
                    if (spriteRenderer != null && !isInvincible) spriteRenderer.color = colorOriginal;
                }
            }
        }
    }

    // --- NUEVA LÓGICA DIRECCIONAL DE SALTO ---
    private void IntentarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isGrabbingWall)
        {
            currentStamina -= staminaJumpCost;
            if (currentStamina < 0) currentStamina = 0;

            // Leemos si el jugador está presionando Izquierda o Derecha
            float inputX = inputActions.Player.Move.ReadValue<Vector2>().x;
            float direccionSaltoX = 0f;

            if (Mathf.Abs(inputX) > 0.1f)
            {
                // Salta hacia donde apunta el input (A o D)
                direccionSaltoX = Mathf.Sign(inputX);
            }
            else
            {
                // Si no toca ninguna flecha, salta hacia donde está mirando por defecto
                direccionSaltoX = transform.localScale.x > 0 ? 1 : -1;
            }

            // Aplicamos la fuerza del salto
            rb.linearVelocity = new Vector2(direccionSaltoX * speed * 0.8f, jumpForce);

            DesactivarAgarre();
            coyoteTimeCounter = 0f;

            // Activamos el "Anti-Imán" para no re-agarrar la pared instantáneamente
            wallJumpTimer = wallJumpCooldown;
            return;
        }

        if (coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
        }
    }

    private void CancelarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (rb.linearVelocity.y > 0 && !isGrabbingWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

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
            DesactivarAgarre();

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForceValue, ForceMode2D.Impulse);
        }
    }

    public void Revivir()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        if (health != null) health.Init(maxHealth);

        transform.position = ultimoCheckpoint;
        DesactivarAgarre();
        rb.linearVelocity = Vector2.zero;
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
                DesactivarAgarre();
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
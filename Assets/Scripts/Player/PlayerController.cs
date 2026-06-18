using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Efectos de Sonido (SFX)")]
    public AudioSource audioSource;
    public AudioClip sfxSalto;
    public AudioClip sfxDisparo;
    public AudioClip sfxCaidaOMuerte;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxPasos;
    [Tooltip("Tiempo entre cada sonido de paso al correr")]
    public float tiempoEntrePasos = 0.3f;
    private float timerPasos;

    [Header("Salud y Reaparición")]
    PlayerHealth health;
    public int maxHealth = 6;
    public int currentHealth;
    public Vector2 ultimoCheckpoint;

    [Header("UI de Corazones")]
    public Image[] corazonesUI;
    public Sprite corazonLleno;
    public Sprite corazonMitad;
    public Sprite corazonVacio;

    [Header("Efectos de Muerte")]
    public Image pantallaNegra;
    public bool isDead = false;
    [Tooltip("Tiempo que espera el código para mostrar la pantalla negra tras morir.")]
    public float duracionAnimacionMuerte = 1f;

    [Header("Bloqueos de Estado")]
    private bool isAttacking = false;

    [Header("I-Frames (Inmortalidad)")]
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    [Header("Game Feel")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Sincronización de Ataque")]
    public float delayDisparo = 0.3f;

    [Header("Sistema de Estamina y Escalada")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainHold = 10f;
    public float staminaDrainClimb = 20f;
    public float staminaJumpCost = 30f;
    public float climbSpeed = 5f;

    [Tooltip("Fuerza del salto exclusivamente al despegarse de una pared")]
    public float wallJumpForce = 16f;
    public float wallJumpCooldown = 0.2f;
    private float wallJumpTimer;

    [Range(0f, 1f)]
    public float umbralEstaminaBaja = 0.25f;
    public Slider barraEstamina;

    private bool isGrabbingWall = false;
    private bool canGrab = true;
    private float originalGravityScale;
    private Color colorOriginalBarra;

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
    public float distanciaSuelo = 0.3f;
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

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        ActualizarCorazones();
        ultimoCheckpoint = transform.position;
        originalGravityScale = rb.gravityScale;
        currentStamina = maxStamina;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue = maxStamina;
            barraEstamina.value = maxStamina;
            barraEstamina.gameObject.SetActive(false);

            Image fillImage = barraEstamina.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                colorOriginalBarra = fillImage.color;
            }
        }

        if (pantallaNegra != null)
        {
            pantallaNegra.color = new Color(0, 0, 0, 0);
            pantallaNegra.gameObject.SetActive(false);
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

            inputActions.Player.Attack.performed += AnimarAtaque;

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
            inputActions.Player.Attack.performed -= AnimarAtaque;

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
        if (isDead) return;

        if (movement != null && !isGrabbingWall) movement.Tick();

        if (controladorSuelo != null)
        {
            enSuelo = Physics2D.Raycast(controladorSuelo.position, Vector2.down, distanciaSuelo, capaSuelo);
        }

        if (controladorPared != null)
        {
            float direccionMiradaX = transform.localScale.x > 0 ? 1 : -1;
            Vector2 direccionMirada = new Vector2(direccionMiradaX, 0);
            tocandoPared = Physics2D.Raycast(controladorPared.position, direccionMirada, distanciaPared, capaSuelo);
        }

        if (!isGrabbingWall)
        {
            float inputX = inputActions.Player.Move.ReadValue<Vector2>().x;

            if (animator != null) animator.SetFloat("Movement", Mathf.Abs(inputX));

            if (inputX > 0.1f) transform.localScale = new Vector3(1, 1, 1);
            else if (inputX < -0.1f) transform.localScale = new Vector3(-1, 1, 1);

            // --- SISTEMA DE PASOS ---
            if (enSuelo && Mathf.Abs(inputX) > 0.1f)
            {
                if (Time.time >= timerPasos)
                {
                    ReproducirSonido(sfxPasos, 0.5f); // 0.5f es el volumen de los pasos para que no aturdan
                    timerPasos = Time.time + tiempoEntrePasos;
                }
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Movement", 0f);
        }

        if (animator != null)
        {
            animator.SetBool("IsGrounded", enSuelo);
            animator.SetBool("IsGrabbing", isGrabbingWall);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

            if (isGrabbingWall)
            {
                float velocidadNormalizada = Mathf.Abs(rb.linearVelocity.y) / climbSpeed;
                animator.SetFloat("ClimbSpeed", velocidadNormalizada);
            }
            else
            {
                animator.SetFloat("ClimbSpeed", 1f);
            }
        }

        bool presionaGanchos = false;

        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.isPressed)
        {
            presionaGanchos = true;
        }

        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            if (UnityEngine.InputSystem.Gamepad.current.rightTrigger.ReadValue() > 0.1f)
            {
                presionaGanchos = true;
            }
        }

        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;

        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, 60f * Time.deltaTime);
            canGrab = true;

            if (barraEstamina != null && barraEstamina.gameObject.activeSelf && currentStamina >= maxStamina)
                barraEstamina.gameObject.SetActive(false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (tocandoPared && !enSuelo && presionaGanchos && canGrab && currentStamina > 0 && wallJumpTimer <= 0)
        {
            if (!isGrabbingWall)
            {
                isGrabbingWall = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (isGrabbingWall) DesactivarAgarre();

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

        if (barraEstamina != null && barraEstamina.gameObject.activeSelf) barraEstamina.value = currentStamina;

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
        if (isDead) return;

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

                    if (isGrabbingWall && spriteRenderer != null && !isInvincible)
                    {
                        spriteRenderer.color = Color.Lerp(colorOriginal, Color.red, 0.4f);
                    }
                }
                else
                {
                    fillImage.color = colorOriginalBarra;
                    if (spriteRenderer != null && !isInvincible) spriteRenderer.color = colorOriginal;
                }
            }
        }
    }

    private void AnimarAtaque(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isDead || isAttacking || isGrabbingWall) return;

        isAttacking = true;

        // --- AQUÍ EL SONIDO FUE ELIMINADO Y MOVIDO A LA RUTINA ---

        if (animator != null) animator.SetTrigger("Attack");
        StartCoroutine(RutinaDisparoSincronizado(context));
    }

    private IEnumerator RutinaDisparoSincronizado(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // 1. Esperamos a que la animación tense el arco/arma
        yield return new WaitForSeconds(delayDisparo);

        // 2. --- SONIDO DE DISPARO EXACTO CON LA FLECHA ---
        ReproducirSonido(sfxDisparo);

        // 3. Sale el proyectil
        if (weapon != null) weapon.OnFire(context);

        float tiempoRestante = fireCooldown - delayDisparo;
        if (tiempoRestante > 0) yield return new WaitForSeconds(tiempoRestante);

        if (animator != null) animator.ResetTrigger("Attack");
        isAttacking = false;
    }

    private void IntentarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isDead) return;

        if (isGrabbingWall)
        {
            currentStamina -= staminaJumpCost;
            if (currentStamina < 0) currentStamina = 0;

            float inputX = inputActions.Player.Move.ReadValue<Vector2>().x;
            float direccionSaltoX = 0f;

            if (Mathf.Abs(inputX) > 0.1f) direccionSaltoX = Mathf.Sign(inputX);
            else direccionSaltoX = transform.localScale.x > 0 ? 1 : -1;

            rb.linearVelocity = new Vector2(direccionSaltoX * speed * 0.8f, wallJumpForce);

            // --- SONIDO DE SALTO (En Pared) ---
            ReproducirSonido(sfxSalto);

            if (animator != null)
            {
                animator.ResetTrigger("JumpTrigger");
                animator.SetTrigger("JumpTrigger");
            }

            DesactivarAgarre();
            coyoteTimeCounter = 0f;
            wallJumpTimer = wallJumpCooldown;
            return;
        }

        if (coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;

            // --- SONIDO DE SALTO (Normal) ---
            ReproducirSonido(sfxSalto);

            if (animator != null)
            {
                animator.ResetTrigger("JumpTrigger");
                animator.SetTrigger("JumpTrigger");
            }
        }
    }

    private void CancelarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (rb.linearVelocity.y > 0 && !isGrabbingWall)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir, float knockbackForceValue)
    {
        if (isInvincible || isDead) return;
        currentHealth -= damageAmount;

        ActualizarCorazones();

        // --- SONIDO AL RECIBIR DAÑO ---
        ReproducirSonido(sfxCaidaOMuerte);

        if (currentHealth <= 0)
        {
            StartCoroutine(SecuenciaMuerte());
        }
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
    public void Curar(int cantidad)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += cantidad;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            ActualizarCorazones();

            
        }
    }
    private IEnumerator SecuenciaMuerte()
    {
        isDead = true;
        isAttacking = false;
        inputActions.Disable();
        DesactivarAgarre();

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null) animator.SetTrigger("Death");

        yield return new WaitForSeconds(duracionAnimacionMuerte);

        float duracionFade = 0.5f;
        float tiempo = 0f;

        if (pantallaNegra != null)
        {
            pantallaNegra.gameObject.SetActive(true);
            while (tiempo < duracionFade)
            {
                tiempo += Time.deltaTime;
                float alphaPantalla = Mathf.Lerp(0f, 1f, tiempo / duracionFade);
                pantallaNegra.color = new Color(0, 0, 0, alphaPantalla);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);

        transform.position = ultimoCheckpoint;
        currentHealth = maxHealth;

        ActualizarCorazones();

        currentStamina = maxStamina;
        if (health != null) health.Init(maxHealth);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetBool("IsGrounded", true);
            animator.SetFloat("Movement", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
        }

        if (pantallaNegra != null)
        {
            tiempo = 0f;
            while (tiempo < duracionFade)
            {
                tiempo += Time.deltaTime;
                float alphaPantalla = Mathf.Lerp(1f, 0f, tiempo / duracionFade);
                pantallaNegra.color = new Color(0, 0, 0, alphaPantalla);
                yield return null;
            }
            pantallaNegra.gameObject.SetActive(false);
        }

        inputActions.Enable();
        isDead = false;

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void ProcesarChoques(GameObject objetoTocado)
    {
        if (movement == null || isDead) return;
        string objName = objetoTocado.name.ToLower();
        string objTag = objetoTocado.tag.ToLower();

        if (objetoTocado.layer == 11)
        {
            currentHealth -= 1;
            ActualizarCorazones();

            // --- SONIDO AL CAERSE EN TRAMPAS ---
            ReproducirSonido(sfxCaidaOMuerte);

            if (currentHealth <= 0) StartCoroutine(SecuenciaMuerte());
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
            // Solo suena si tocás un checkpoint en el que no estabas parado antes
            if (ultimoCheckpoint != (Vector2)objetoTocado.transform.position)
            {
                ReproducirSonido(sfxCheckpoint);
                ultimoCheckpoint = objetoTocado.transform.position;
                movement.SetRespawnPoint(objetoTocado.transform.position);
            }
        }
    }

    private void ActualizarCorazones()
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            int valorDeEsteCorazon = (i + 1) * 2;

            if (currentHealth >= valorDeEsteCorazon)
            {
                corazonesUI[i].sprite = corazonLleno;
            }
            else if (currentHealth == valorDeEsteCorazon - 1)
            {
                corazonesUI[i].sprite = corazonMitad;
            }
            else
            {
                corazonesUI[i].sprite = corazonVacio;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) { ProcesarChoques(collision.gameObject); }
    private void OnTriggerEnter2D(Collider2D collision) { ProcesarChoques(collision.gameObject); }

    // --- FUNCIÓN HELPER PARA LOS SONIDOS ---
    private void ReproducirSonido(AudioClip clip, float volumen = 1f)
    {
        if (audioSource != null && clip != null)
        {
            float volumenFinal = volumen;

            // Si el menú existe, le aplicamos el porcentaje de la barrita de SFX
            if (MusicManager.instance != null)
            {
                volumenFinal = volumen * MusicManager.instance.volumenSFXActual;
            }

            audioSource.PlayOneShot(clip, volumenFinal);
        }
    }
}
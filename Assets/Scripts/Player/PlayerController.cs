using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Efectos de Sonido (SFX)")]
    public AudioSource audioSource;
    public AudioClip sfxSalto;
    public AudioClip sfxDisparo;
    public AudioClip sfxCaidaOMuerte;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxPasos;
    public AudioClip sfxParry;
    public float tiempoEntrePasos = 0.3f;
    private float timerPasos;

    [Header("Mecánica de Parry (Desvío)")]
    public float parryDuration = 0.25f;
    public float parryCooldown = 1f;
    public float radioParry = 1.5f;
    public LayerMask capaProyectiles;

    [Space(5)]
    [Header("Control de Sonido y Sincronización del Parry")]
    [HideInInspector] public AudioClip sfxInicioParry;
    [HideInInspector] public float volumenInicioParry = 0.7f;
    [Range(0f, 1f)] public float volumenParry = 1f;
    public float retrasoSonidoParry = 0f;

    private bool isParrying = false;
    private float parryTimer = 0f;

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
    public float duracionAnimacionMuerte = 1f;

    [Header("Bloqueos de Estado")]
    private bool isAttacking = false;
    private bool isHoldingAttack = false;
    private Coroutine rutinaAtaqueActual;

    [Header("Game Feel (Knockback)")]
    public float knockbackStunDuration = 0.25f;
    private float knockbackStunTimer;
    private float lastFacingDirection = 1f;

    [Header("Sensibilidad de Mando")]
    [Tooltip("Ignora movimientos mínimos para evitar drift al caminar (recomendado: 0.1)")]
    [Range(0f, 1f)] public float stickDeadzone = 0.1f;

    [Header("Control de Cámara (Mirar alrededor)")]
    public Transform cameraTarget;
    public float cameraPanDistance = 4f;
    public float cameraPanSpeed = 6f;
    private Vector3 originalCameraTargetLocalPos;

    [Header("Game Feel (Salto y Disparo Aéreo)")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float tiempoLevitacionAerea = 0.2f;
    private float timerLevitacionAerea;
    private bool yaLevitoEnElAire = false;

    [Header("Sincronización de Ataque")]
    public float delayDisparo = 0.3f;
    public float tiempoParaCongelarAnimacion = 0.15f;
    [Tooltip("En qué porcentaje de la animación se congela (ej: 0.65)")]
    [Range(0f, 1f)] public float frameDeCongelamiento = 0.65f;

    [Header("I-Frames (Inmortalidad)")]
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    [Header("Sistema de Estamina y Escalada")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainHold = 10f;
    public float staminaDrainClimb = 20f;
    public float staminaJumpCost = 30f;
    public float climbSpeed = 5f;

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

        if (rb == null) Debug.LogError("🚨 ERROR CRÍTICO: El objeto carece de Rigidbody2D.");

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

        if (rb != null) originalGravityScale = rb.gravityScale;

        currentStamina = maxStamina;
        lastFacingDirection = transform.localScale.x > 0 ? 1f : -1f;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue = maxStamina;
            barraEstamina.value = maxStamina;
            barraEstamina.gameObject.SetActive(false);

            Image fillImage = barraEstamina.fillRect.GetComponent<Image>();
            if (fillImage != null) colorOriginalBarra = fillImage.color;
        }

        if (pantallaNegra != null)
        {
            pantallaNegra.color = new Color(0, 0, 0, 0);
            pantallaNegra.gameObject.SetActive(false);
        }

        if (cameraTarget != null)
        {
            originalCameraTargetLocalPos = cameraTarget.localPosition;
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

            inputActions.Player.Attack.started += IniciarApuntado;
            inputActions.Player.Attack.canceled += SoltarDisparo;

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
            inputActions.Player.Attack.started -= IniciarApuntado;
            inputActions.Player.Attack.canceled -= SoltarDisparo;
            inputActions.Player.Move.performed -= weapon.OnMove;
            inputActions.Player.AltAttack.performed -= weapon.OnAltFire;
            inputActions.Player.Move.performed -= movement.OnMove;
            inputActions.Player.Move.canceled -= movement.OnMove;
            inputActions.Player.Jump.performed -= IntentarSalto;
            inputActions.Player.Jump.canceled -= CancelarSalto;
            inputActions.Disable();
        }
    }

    private Vector2 LeerStickIzquierdo()
    {
        Vector2 rawInput = Vector2.zero;

        // 🔥 LECTURA CRUDA DE HARDWARE: 
        // Evita el "Axis Snapping" de Unity que fuerza a 0 el eje X cuando mirás hacia abajo.
        if (Gamepad.current != null)
        {
            rawInput = Gamepad.current.leftStick.ReadValue();
        }
        else if (inputActions != null)
        {
            rawInput = inputActions.Player.Move.ReadValue<Vector2>();
        }

        return rawInput.magnitude >= stickDeadzone ? rawInput : Vector2.zero;
    }

    private Vector2 LeerStickDerecho()
    {
        if (Gamepad.current == null) return Vector2.zero;
        Vector2 rawInput = Gamepad.current.rightStick.ReadValue();
        return rawInput.magnitude >= stickDeadzone ? rawInput : Vector2.zero;
    }

    private void Update()
    {
        if (inputActions == null || rb == null) return;
        if (isDead || Time.timeScale == 0f) return;

        if (knockbackStunTimer > 0) knockbackStunTimer -= Time.deltaTime;
        if (parryTimer > 0) parryTimer -= Time.deltaTime;
        if (timerLevitacionAerea > 0) timerLevitacionAerea -= Time.deltaTime;

        Vector2 moveInput = LeerStickIzquierdo();
        Vector2 cameraPanInput = LeerStickDerecho();

        if (cameraTarget != null)
        {
            Vector3 targetOffset = new Vector3(cameraPanInput.x, cameraPanInput.y, 0f) * cameraPanDistance;
            Vector3 desiredPosition = originalCameraTargetLocalPos + targetOffset;

            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, desiredPosition, Time.deltaTime * cameraPanSpeed);
        }

        bool presionaParry = false;
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) presionaParry = true;
        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) presionaParry = true;

        if (presionaParry && parryTimer <= 0 && !isParrying && !isGrabbingWall && !isAttacking && knockbackStunTimer <= 0)
        {
            InterrumpirAtaque();
            StartCoroutine(RutinaParry());
        }

        if (movement != null && !isGrabbingWall) movement.Tick();

        if (controladorSuelo != null) enSuelo = Physics2D.Raycast(controladorSuelo.position, Vector2.down, distanciaSuelo, capaSuelo);

        if (controladorPared != null)
        {
            float direccionMiradaX = transform.localScale.x > 0 ? 1 : -1;
            Vector2 direccionMirada = new Vector2(direccionMiradaX, 0);
            tocandoPared = Physics2D.Raycast(controladorPared.position, direccionMirada, distanciaPared, capaSuelo);
        }

        if (knockbackStunTimer <= 0)
        {
            if (!isGrabbingWall)
            {
                float inputX = moveInput.x;

                // 🔥 MICRO-DEADZONE (0.05f) 
                // Ignora el pulso físico de tu dedo, pero si empujás un "poquito", gira al instante.
                if (Mathf.Abs(inputX) > 0.05f)
                {
                    lastFacingDirection = (inputX > 0f) ? 1f : -1f;
                }

                // Aplicación inmediata del giro
                Vector3 escalaForzada = transform.localScale;
                escalaForzada.x = lastFacingDirection;
                transform.localScale = escalaForzada;

                float animatorSpeedValue = Mathf.Abs(inputX) > 0.1f ? 1f : 0f;

                if (isHoldingAttack || isAttacking)
                {
                    animatorSpeedValue = 0f;
                }

                if (animator != null) animator.SetFloat("Movement", animatorSpeedValue);

                if (enSuelo && Mathf.Abs(inputX) > 0.1f)
                {
                    if (Time.time >= timerPasos)
                    {
                        ReproducirSonido(sfxPasos, 0.5f);
                        timerPasos = Time.time + tiempoEntrePasos;
                    }
                }
            }
            else
            {
                if (animator != null) animator.SetFloat("Movement", 0f);
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Movement", 0f);
        }

        if (firePoint != null)
        {
            firePoint.localRotation = Quaternion.identity;
        }

        if (animator != null)
        {
            animator.SetBool("IsGrounded", enSuelo);
            animator.SetBool("IsGrabbing", isGrabbingWall);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool estaEnAtaque = stateInfo.IsName("Attack");

            if (isHoldingAttack && estaEnAtaque)
            {
                if (stateInfo.normalizedTime >= frameDeCongelamiento)
                {
                    animator.speed = 0f;
                    animator.Play("Attack", 0, frameDeCongelamiento);
                }
                else
                {
                    animator.speed = 1f;
                }
            }
            else
            {
                animator.speed = (Time.timeScale == 0f) ? 0f : 1f;
            }

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
        if (Keyboard.current != null && Keyboard.current.eKey.isPressed) presionaGanchos = true;
        if (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.1f) presionaGanchos = true;

        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;

        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, 60f * Time.deltaTime);
            canGrab = true;

            yaLevitoEnElAire = false;

            if (barraEstamina != null && barraEstamina.gameObject.activeSelf && currentStamina >= maxStamina)
                barraEstamina.gameObject.SetActive(false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (tocandoPared && !enSuelo && presionaGanchos && canGrab && currentStamina > 0 && wallJumpTimer <= 0 && knockbackStunTimer <= 0)
        {
            if (!isGrabbingWall)
            {
                isGrabbingWall = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;

                yaLevitoEnElAire = false;
            }
        }
        else if (isGrabbingWall)
        {
            Vector2 inputsMovimiento = LeerStickIzquierdo();

            if (!tocandoPared && inputsMovimiento.y > 0)
            {
                DesactivarAgarre();
                InterrumpirAtaque();
                if (animator != null)
                {
                    animator.ResetTrigger("JumpTrigger");
                    animator.SetTrigger("JumpTrigger");
                }
                ReproducirSonido(sfxSalto);
                rb.linearVelocity = new Vector2(lastFacingDirection * speed * 0.6f, jumpForce * 0.75f);
            }
            else
            {
                DesactivarAgarre();
            }
        }

        if (isGrabbingWall)
        {
            if (barraEstamina != null && !barraEstamina.gameObject.activeSelf)
                barraEstamina.gameObject.SetActive(true);

            Vector2 inputsMovimiento = LeerStickIzquierdo();
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

        if (isInvincible && !isParrying)
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
        if (inputActions == null || rb == null || isDead || Time.timeScale == 0f) return;

        if (isGrabbingWall)
        {
            Vector2 inputsMovimiento = LeerStickIzquierdo();
            rb.linearVelocity = new Vector2(0f, inputsMovimiento.y * climbSpeed);
        }
        else if (knockbackStunTimer <= 0)
        {
            if (movement != null) movement.FixedTick();

            if (timerLevitacionAerea > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }
    }

    private void InterrumpirAtaque()
    {
        if (isAttacking || isHoldingAttack)
        {
            isAttacking = false;
            isHoldingAttack = false;
            if (rutinaAtaqueActual != null) StopCoroutine(rutinaAtaqueActual);

            if (animator != null)
            {
                animator.speed = 1f;
                animator.SetBool("IsHoldingAttack", false);
                animator.ResetTrigger("Attack");
            }
        }
    }

    private void IniciarApuntado(InputAction.CallbackContext context)
    {
        if (isDead || isAttacking || isGrabbingWall || Time.timeScale == 0f || isParrying || knockbackStunTimer > 0) return;

        isAttacking = true;
        isHoldingAttack = true;

        if (!enSuelo && !yaLevitoEnElAire)
        {
            timerLevitacionAerea = tiempoLevitacionAerea;
            yaLevitoEnElAire = true;
        }

        if (animator != null)
        {
            animator.SetBool("IsHoldingAttack", true);
            animator.SetTrigger("Attack");
        }

        if (rutinaAtaqueActual != null) StopCoroutine(rutinaAtaqueActual);
        rutinaAtaqueActual = StartCoroutine(RutinaDisparoSincronizado(context));
    }

    private void SoltarDisparo(InputAction.CallbackContext context)
    {
        if (!isHoldingAttack) return;

        isHoldingAttack = false;
        if (animator != null)
        {
            animator.SetBool("IsHoldingAttack", false);
            animator.speed = 1f;
        }
    }

    private IEnumerator RutinaDisparoSincronizado(InputAction.CallbackContext context)
    {
        float timer = 0f;
        bool animacionEstaCongelada = false;

        while (isHoldingAttack || timer < delayDisparo)
        {
            timer += Time.deltaTime;

            if (isHoldingAttack && timer >= tiempoParaCongelarAnimacion && !animacionEstaCongelada)
            {
                if (animator != null)
                {
                    animator.speed = 0f;
                    animator.Play("Attack", 0, frameDeCongelamiento);
                }
                animacionEstaCongelada = true;
            }

            if (!isHoldingAttack && timer >= delayDisparo)
            {
                break;
            }

            yield return null;
        }

        if (animator != null) animator.speed = 1f;

        Vector3 escalaForzada = transform.localScale;
        escalaForzada.x = lastFacingDirection;
        transform.localScale = escalaForzada;

        if (firePoint != null)
        {
            firePoint.localRotation = Quaternion.identity;
        }

        ReproducirSonido(sfxDisparo);
        if (weapon != null) weapon.OnFire(context);

        float tiempoRestante = fireCooldown - timer;
        if (tiempoRestante > 0) yield return new WaitForSeconds(tiempoRestante);

        if (animator != null) animator.ResetTrigger("Attack");
        isAttacking = false;
    }

    private IEnumerator RutinaParry()
    {
        isParrying = true;
        parryTimer = parryCooldown;

        if (animator != null) animator.SetTrigger("Parry");

        float timer = 0f;
        while (timer < parryDuration)
        {
            Collider2D[] proyectiles = Physics2D.OverlapCircleAll(transform.position, radioParry, capaProyectiles);

            foreach (Collider2D proy in proyectiles)
            {
                if (proy.CompareTag("Parried")) continue;

                Rigidbody2D rbProy = proy.GetComponent<Rigidbody2D>();
                if (rbProy != null)
                {
                    float desvioX = Random.Range(-5f, 5f);
                    rbProy.linearVelocity = new Vector2(desvioX, 15f);

                    float angulo = Mathf.Atan2(rbProy.linearVelocity.y, rbProy.linearVelocity.x) * Mathf.Rad2Deg;
                    proy.transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);

                    proy.tag = "Parried";
                    proy.SendMessage("ResetLife", SendMessageOptions.DontRequireReceiver);

                    if (sfxParry != null)
                    {
                        if (retrasoSonidoParry > 0f)
                        {
                            StartCoroutine(EjecutarSonidoConRetraso(sfxParry, volumenParry, retrasoSonidoParry));
                        }
                        else
                        {
                            ReproducirSonidoConfigurado(sfxParry, volumenParry);
                        }
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isParrying = false;
        if (spriteRenderer != null && !isInvincible) spriteRenderer.color = colorOriginal;
    }

    private IEnumerator EjecutarSonidoConRetraso(AudioClip clip, float volumen, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReproducirSonidoConfigurado(clip, volumen);
    }

    private void ReproducirSonidoConfigurado(AudioClip clip, float volumenEspecifico)
    {
        if (audioSource == null || clip == null) return;

        float volumenFinal = volumenEspecifico;

        if (MusicManager.instance != null)
        {
            volumenFinal = volumenEspecifico * MusicManager.instance.volumenSFXActual;
        }

        audioSource.PlayOneShot(clip, volumenFinal);
    }

    private void DesactivarAgarre()
    {
        isGrabbingWall = false;
        if (rb != null) rb.gravityScale = originalGravityScale;
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

    private void IntentarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (inputActions == null || rb == null || isDead || Time.timeScale == 0f || knockbackStunTimer > 0) return;

        InterrumpirAtaque();

        if (isGrabbingWall)
        {
            currentStamina -= staminaJumpCost;
            if (currentStamina < 0) currentStamina = 0;

            float inputX = LeerStickIzquierdo().x;
            float direccionSaltoX = 0f;

            if (Mathf.Abs(inputX) > 0f) direccionSaltoX = Mathf.Sign(inputX);
            else direccionSaltoX = transform.localScale.x > 0 ? 1 : -1;

            rb.linearVelocity = new Vector2(direccionSaltoX * speed * 0.8f, wallJumpForce);
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
        if (rb == null || isDead || Time.timeScale == 0f) return;

        if (rb.linearVelocity.y > 0 && !isGrabbingWall)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    public void Curar(int cantidad)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += cantidad;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            ActualizarCorazones();
            if (health != null) health.Init(maxHealth);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir, float knockbackForceValue)
    {
        if (rb == null || isInvincible || isDead || isParrying) return;

        InterrumpirAtaque();

        currentHealth -= damageAmount;
        ActualizarCorazones();
        ReproducirSonido(sfxCaidaOMuerte);

        if (currentHealth <= 0)
        {
            StartCoroutine(SecuenciaMuerte());
        }
        else
        {
            if (health != null)
            {
                try { health.TakeDamage(damageAmount, knockbackDir, knockbackForceValue); }
                catch (System.Exception e) { Debug.LogWarning("Se evitó error de PlayerHealth: " + e.Message); }
            }

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            knockbackStunTimer = knockbackStunDuration;
            DesactivarAgarre();

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForceValue, ForceMode2D.Impulse);
        }
    }

    private IEnumerator SecuenciaMuerte()
    {
        isDead = true;
        InterrumpirAtaque();
        if (inputActions != null) inputActions.Disable();
        DesactivarAgarre();

        yaLevitoEnElAire = false;

        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

        if (inputActions != null) inputActions.Enable();
        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void ProcesarChoques(GameObject objetoTocado)
    {
        if (movement == null || isDead || rb == null) return;
        string objName = objetoTocado.name.ToLower();
        string objTag = objetoTocado.tag.ToLower();

        if (objetoTocado.layer == 11)
        {
            if (isInvincible || isDead) return;

            InterrumpirAtaque();

            currentHealth -= 1;
            ActualizarCorazones();
            ReproducirSonido(sfxCaidaOMuerte);

            if (currentHealth <= 0)
            {
                StartCoroutine(SecuenciaMuerte());
            }
            else
            {
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;
                transform.position = ultimoCheckpoint;
                DesactivarAgarre();
                rb.linearVelocity = Vector2.zero;

                if (health != null)
                {
                    try { health.TakeDamage(1, Vector2.zero, 0f); }
                    catch { }
                }
            }
            return;
        }

        if (objName.Contains("checkpoint") || objTag.Contains("checkpoint"))
        {
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
            if (currentHealth >= valorDeEsteCorazon) corazonesUI[i].sprite = corazonLleno;
            else if (currentHealth == valorDeEsteCorazon - 1) corazonesUI[i].sprite = corazonMitad;
            else corazonesUI[i].sprite = corazonVacio;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) { ProcesarChoques(collision.gameObject); }
    private void OnTriggerEnter2D(Collider2D collision) { ProcesarChoques(collision.gameObject); }

    private void ReproducirSonido(AudioClip clip, float volumen = 1f)
    {
        if (audioSource == null || clip == null) return;

        float volumenFinal = volumen;

        if (MusicManager.instance != null)
        {
            volumenFinal = volumen * MusicManager.instance.volumenSFXActual;
        }

        audioSource.PlayOneShot(clip, volumenFinal);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioParry);
    }
}
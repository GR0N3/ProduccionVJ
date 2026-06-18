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
    [Tooltip("Duración de la ventana de invulnerabilidad y desvío (Ej: 0.25 segundos)")]
    public float parryDuration = 0.25f;
    [Tooltip("Tiempo de espera antes de poder hacer otro parry")]
    public float parryCooldown = 1f;
    [Tooltip("Radio del escudo invisible que detecta la flecha")]
    public float radioParry = 1.5f;
    [Tooltip("Capa donde están las balas/flechas de los enemigos")]
    public LayerMask capaProyectiles;

    [Space(5)]
    [Header("Control de Sonido y Sincronización del Parry")]
    [HideInInspector] public AudioClip sfxInicioParry;
    [HideInInspector] public float volumenInicioParry = 0.7f;
    [Range(0f, 1f)]
    [Tooltip("Volumen del sonido de éxito (cuando desvía el proyectil)")]
    public float volumenParry = 1f;
    [Tooltip("Tiempo de retraso en segundos para el sonido de éxito (0 = instantáneo al chocar)")]
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

    [Header("Game Feel")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float knockbackStunDuration = 0.25f;
    private float knockbackStunTimer;
    private Vector3 escalaAlRecibirDaño;

    [Header("I-Frames (Inmortalidad)")]
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    [Header("Sincronización de Ataque")]
    public float delayDisparo = 0.3f;

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
        if (isDead || Time.timeScale == 0f) return;

        if (knockbackStunTimer > 0) knockbackStunTimer -= Time.deltaTime;
        if (parryTimer > 0) parryTimer -= Time.deltaTime;

        bool presionaParry = false;
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) presionaParry = true;
        if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) presionaParry = true;

        if (presionaParry && parryTimer <= 0 && !isParrying && !isGrabbingWall && !isAttacking && knockbackStunTimer <= 0)
        {
            StartCoroutine(RutinaParry());
        }

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

        if (knockbackStunTimer <= 0)
        {
            if (!isGrabbingWall)
            {
                float inputX = inputActions.Player.Move.ReadValue<Vector2>().x;

                if (animator != null) animator.SetFloat("Movement", Mathf.Abs(inputX));

                if (inputX > 0.1f) transform.localScale = new Vector3(1, 1, 1);
                else if (inputX < -0.1f) transform.localScale = new Vector3(-1, 1, 1);

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

        if (Keyboard.current != null && Keyboard.current.eKey.isPressed) presionaGanchos = true;
        if (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.1f) presionaGanchos = true;

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

        if (tocandoPared && !enSuelo && presionaGanchos && canGrab && currentStamina > 0 && wallJumpTimer <= 0 && knockbackStunTimer <= 0)
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

    private void LateUpdate()
    {
        if (knockbackStunTimer > 0)
        {
            transform.localScale = escalaAlRecibirDaño;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || Time.timeScale == 0f) return;

        if (isGrabbingWall)
        {
            Vector2 inputsMovimiento = inputActions.Player.Move.ReadValue<Vector2>();
            rb.linearVelocity = new Vector2(0f, inputsMovimiento.y * climbSpeed);
        }
        else if (knockbackStunTimer <= 0)
        {
            if (movement != null) movement.FixedTick();
        }
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
        if (audioSource != null && clip != null)
        {
            float volumenGlobalSFX = 1f;

            if (MusicManager.instance != null)
            {
                volumenGlobalSFX = MusicManager.instance.volumenSFXActual;
            }

            audioSource.PlayOneShot(clip, volumenEspecifico * volumenGlobalSFX);
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
        if (isDead || isAttacking || isGrabbingWall || Time.timeScale == 0f || isParrying || knockbackStunTimer > 0) return;

        isAttacking = true;

        if (animator != null) animator.SetTrigger("Attack");
        StartCoroutine(RutinaDisparoSincronizado(context));
    }

    private IEnumerator RutinaDisparoSincronizado(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        yield return new WaitForSeconds(delayDisparo);

        ReproducirSonido(sfxDisparo);

        if (weapon != null) weapon.OnFire(context);

        float tiempoRestante = fireCooldown - delayDisparo;
        if (tiempoRestante > 0) yield return new WaitForSeconds(tiempoRestante);

        if (animator != null) animator.ResetTrigger("Attack");
        isAttacking = false;
    }

    private void IntentarSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isDead || Time.timeScale == 0f || knockbackStunTimer > 0) return;

        if (isGrabbingWall)
        {
            currentStamina -= staminaJumpCost;
            if (currentStamina < 0) currentStamina = 0;

            float inputX = inputActions.Player.Move.ReadValue<Vector2>().x;
            float direccionSaltoX = 0f;

            if (Mathf.Abs(inputX) > 0.1f) direccionSaltoX = Mathf.Sign(inputX);
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
        if (isDead || Time.timeScale == 0f) return;

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

            // 🔥 SOLUCIÓN 1: Mantiene sincronizado al script oculto para que no muera en falso
            if (health != null) health.Init(maxHealth);
        }
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir, float knockbackForceValue)
    {
        if (isInvincible || isDead || isParrying) return;

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
                // 🔥 SOLUCIÓN 2: Try-Catch protector para enemigos normales
                try { health.TakeDamage(damageAmount, knockbackDir, knockbackForceValue); }
                catch (System.Exception e) { Debug.LogWarning("Se evitó error de PlayerHealth: " + e.Message); }
            }

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            escalaAlRecibirDaño = transform.localScale;
            knockbackStunTimer = knockbackStunDuration;

            DesactivarAgarre();

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForceValue, ForceMode2D.Impulse);
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
            // 🔥 SOLUCIÓN 3: Evitar bofetadas dobles en la trampa en el mismo segundo
            if (isInvincible || isDead) return;

            currentHealth -= 1;
            ActualizarCorazones();

            ReproducirSonido(sfxCaidaOMuerte);

            if (currentHealth <= 0)
            {
                StartCoroutine(SecuenciaMuerte());
            }
            else
            {
                // Damos invulnerabilidad primero
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;

                // 🔥 SOLUCIÓN 4: Teletransportamos ANTES de comunicarnos con el script frágil
                transform.position = ultimoCheckpoint;
                DesactivarAgarre();
                rb.linearVelocity = Vector2.zero;

                if (health != null)
                {
                    try { health.TakeDamage(1, Vector2.zero, 0f); }
                    catch { /* Se ahoga el crasheo para no frenar el teleport */ }
                }
            }

            return; // Corta la ejecución limpia acá
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

    private void ReproducirSonido(AudioClip clip, float volumen = 1f)
    {
        if (audioSource != null && clip != null)
        {
            float volumenFinal = volumen;

            if (MusicManager.instance != null)
            {
                volumenFinal = volumen * MusicManager.instance.volumenSFXActual;
            }

            audioSource.PlayOneShot(clip, volumenFinal);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioParry);
    }
}
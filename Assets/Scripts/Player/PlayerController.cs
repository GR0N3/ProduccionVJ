using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class PlayerController : MonoBehaviour
{
    private const string IdleAnimation = "Idle";
    private const string WalkAnimation = "Walk";
    private const string AttackAnimation = "Attack";
    private const string DeathAnimation = "DeadP";

    private PlayerManager playerManager;
    private string currentAnimation;
    private bool isDead;
    private bool isInitialized;
    private bool areInputsBound;
    private Coroutine initializationCoroutine;
    private float attackLockUntil;

    private PlayerHealth health;
    public int maxHealth => (int)playerManager.Stats.GetStat(UpgradeType.MaxHealth);

    private PlayerWeapon weapon;
    [SerializeField] private GameObject bulletPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    public int Damage => (int)playerManager.Stats.GetStat(UpgradeType.Damage);
    public int BulletsCount => (int)playerManager.Stats.GetStat(UpgradeType.BullesCount);
    public float BulletSpread => playerManager.Stats.GetStat(UpgradeType.BulletsSpread);
    public float BulletSpeed => playerManager.Stats.GetStat(UpgradeType.BulletSpeed);
    public float KnockbackForce => playerManager.Stats.GetStat(UpgradeType.KnockbackForce);

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    private PlayerMovement movement;
    public Rigidbody2D rb;
    [SerializeField] private Animator animator;
    public float Speed => playerManager.Stats.GetStat(UpgradeType.Speed);
    public float JumpForce => playerManager.Stats.GetStat(UpgradeType.JumpForce);
    public float Acceleration => playerManager.Stats.GetStat(UpgradeType.Acceleration);
    public float Deceleration => playerManager.Stats.GetStat(UpgradeType.Deceleration);

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask borderLayer;

    public LayerMask GroundLayer => groundLayer;
    public LayerMask BorderLayer => borderLayer;

    public InputSystem_Actions InputActions { get; private set; }

    private void Awake()
    {
        InputActions ??= new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartInitializationIfNeeded();
    }

    private void OnDisable()
    {
        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        if (!areInputsBound)
        {
            return;
        }

        areInputsBound = false;
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        InputActions.Disable();
    }

    private void Update()
    {
        if (!isInitialized || isDead)
        {
            return;
        }

        ProcessPlayerInput();
        movement.Tick();
        UpdateMovementAnimation();
    }

    private void FixedUpdate()
    {
        if (!isInitialized || isDead)
        {
            return;
        }

        movement.FixedTick();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isInitialized || isDead)
        {
            return;
        }

        if ((collision.gameObject.layer == 8))
        {
            health.TakeDamage(1, new Vector2(-1, -1), 25f);
        }
    }

    public void PlayAttackAnimation()
    {
        if (isDead || animator == null)
        {
            return;
        }

        attackLockUntil = Time.time + GetAnimationLength(AttackAnimation, 0.15f);
        ChangeAnimation(AttackAnimation, 0.05f);
    }

    private void HandlePlayerDeath()
    {
        if (!isActiveAndEnabled || isDead)
        {
            return;
        }

        isDead = true;
        attackLockUntil = 0f;
        rb.linearVelocity = Vector2.zero;
        InputActions.Disable();

        if (animator != null)
        {
            ChangeAnimation(DeathAnimation, 0.05f);
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(GetAnimationLength(DeathAnimation, 0.4f));

        SceneController.Instance
            .NewTransition()
            .Unload(SceneDataBase.Scenes.Match)
            .Unload(SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perfrom();
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || Time.time < attackLockUntil)
        {
            return;
        }

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.05f;
        ChangeAnimation(isMoving ? WalkAnimation : IdleAnimation, 0.05f);
    }

    private void ChangeAnimation(string animationName, float crossFadeDuration)
    {
        if (currentAnimation == animationName)
        {
            return;
        }

        currentAnimation = animationName;
        animator.CrossFade(animationName, crossFadeDuration);
    }

    private float GetAnimationLength(string animationName, float fallback)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return fallback;
        }

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        return fallback;
    }

    private void StartInitializationIfNeeded()
    {
        if (isInitialized || initializationCoroutine != null)
        {
            return;
        }

        initializationCoroutine = StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (!TryInitialize())
        {
            yield return null;
        }

        initializationCoroutine = null;
        BindInputs();
    }

    private bool TryInitialize()
    {
        if (isInitialized || !TryResolvePlayerManager())
        {
            return isInitialized;
        }

        weapon = playerManager.PlayerWeapon;
        health = playerManager.PlayerHealth;
        movement = playerManager.PlayerMovement;

        if (weapon == null || health == null || movement == null)
        {
            return false;
        }

        weapon.Init(this);
        movement.Init(this);
        isInitialized = true;
        return true;
    }

    private void BindInputs()
    {
        if (!isInitialized || areInputsBound)
        {
            return;
        }

        areInputsBound = true;
        InputActions.Enable();
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void ProcessPlayerInput()
    {
        Vector2 moveInput = InputActions.Player.Move.ReadValue<Vector2>();
        movement.SetMoveInput(moveInput);
        weapon.SetMoveInput(moveInput);

        if (InputActions.Player.Jump.WasPressedThisFrame())
        {
            movement.Jump();
        }

        if (InputActions.Player.Attack.WasPressedThisFrame())
        {
            weapon.Fire();
        }

        if (InputActions.Player.AltAttack.WasPressedThisFrame())
        {
            weapon.AltFire();
        }
    }

    private bool TryResolvePlayerManager()
    {
        if (playerManager != null)
        {
            return true;
        }

        if (ServiceLocator.TryGet(out PlayerManager registeredPlayerManager))
        {
            playerManager = registeredPlayerManager;
            return true;
        }

        playerManager = FindAnyObjectByType<PlayerManager>();

        if (playerManager != null)
        {
            ServiceLocator.Register(playerManager);
            return true;
        }

        return false;
    }
}

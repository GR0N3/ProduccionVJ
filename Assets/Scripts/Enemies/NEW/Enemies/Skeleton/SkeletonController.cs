using UnityEngine;
using Enemies.Skeleton.StateMachine;

namespace Enemies.Skeleton
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    public class SkeletonController : MonoBehaviour , IDamageable
    {
        [Header("Enemy Type")]
        public EnemyType EnemyType;

        [Header("Patrol Settings")]
        public LayerMask GroundLayer;
        public float PatrolDistance1 = 1f;
        public float PatrolDistance2 = 2f;
        public float IdleWaitTime = 1f;
        [Tooltip("Distancia hacia adelante desde la que se lanza el raycast de borde de plataforma")]
        public float EdgeCheckDistance = 0.4f;
        [Tooltip("Longitud del raycast hacia abajo para detectar suelo")]
        public float EdgeRayLength = 1.5f;

        [Header("Player Detection")]
        public Transform Player;
        public LayerMask PlayerLayer;

        [Header("Components")]
        public Rigidbody2D Rigidbody { get; private set; }
        public Animator Animator { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public Collider2D[] Colliders { get; private set; }

        [Header("State Machine")]
        public StateMachine.StateMachine StateMachine { get; private set; }
        public SkeletonIdleState IdleState { get; private set; }
        public SkeletonWalkState WalkState { get; private set; }
        public SkeletonAttackState AttackState { get; private set; }
        public SkeletonHitState HitState { get; private set; }
        public SkeletonDeadState DeadState { get; private set; }

        [Header("Effects")]
        [SerializeField] private DamageEffects damageEffects;

        [Header("State Flags")]
        public bool IsDead = false;
        public bool IsHit = false;
        public bool PlayerDetected = false;
        public bool IsInAttackRange = false;
        public bool IsWaiting = false;
        public bool HitWall = false;

        [Header("Current Stats")]
        public float CurrentHealth;


        private Vector2 startPosition;
        private Vector2 movementDirection;
        private Vector2 previousDirection;
        private float currentSpeed;
        private float currentPatrolDistance;
        private bool usingFirstDistance = true;
        private float waitTimer = 0f;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Animator = GetComponent<Animator>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            AudioSource = GetComponent<AudioSource>();
            Colliders = GetComponents<Collider2D>();

            if (EnemyType != null)
            {
                CurrentHealth = EnemyType.MaxHealth;
            }

            startPosition = transform.position;
            movementDirection = Vector2.right;
            currentPatrolDistance = PatrolDistance1;

            StateMachine = new StateMachine.StateMachine();
            IdleState = new SkeletonIdleState(this, StateMachine);
            WalkState = new SkeletonWalkState(this, StateMachine);
            AttackState = new SkeletonAttackState(this, StateMachine);
            HitState = new SkeletonHitState(this, StateMachine);
            DeadState = new SkeletonDeadState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(WalkState);
        }

        private void Update()
        {
            if (IsDead && StateMachine.CurrentState != DeadState)
            {
                DropItem();
                StateMachine.ChangeState(DeadState);
                return;
            }

            CheckPlayerDetection();
            CheckAttackRange();
            StateMachine.CurrentState.Update();
        }

        private void FixedUpdate()
        {
            StateMachine.CurrentState.FixedUpdate();
        }

        private void DropItem()
        {
            var dropChance = 100f;

            if (EnemyType.Drops.Length == 0)
            {
                return;
            }

            float probabilidadAleatoria =
                UnityEngine.Random.Range(0f, 100f);

            if (probabilidadAleatoria <= dropChance)
            {
                int index =
                    UnityEngine.Random.Range(
                        0,
                        EnemyType.Drops.Length
                    );

                Instantiate(
                    EnemyType.Drops[index],
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        private void CheckPlayerDetection()
        {
            if (Player == null)
            {
                PlayerDetected = false;
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
            PlayerDetected = distanceToPlayer <= EnemyType.DetectionRange;
        }

        private void CheckAttackRange()
        {
            if (Player == null)
            {
                IsInAttackRange = false;
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
            IsInAttackRange = distanceToPlayer <= EnemyType.AttackRange;
        }

        public void Patrol()
        {
            if (IsWaiting)
            {
                waitTimer += Time.deltaTime;
                movementDirection = Vector2.zero;
                currentSpeed = 0f;

                if (waitTimer >= IdleWaitTime)
                {
                    IsWaiting = false;
                    waitTimer = 0f;

                    // Girar
                    movementDirection = -previousDirection;
                    startPosition = transform.position;

                    // Alternar entre distancia 1 y 2
                    usingFirstDistance = !usingFirstDistance;
                    currentPatrolDistance = usingFirstDistance ? PatrolDistance1 : PatrolDistance2;
                }
                return;
            }

            currentSpeed = EnemyType.WalkSpeed;

            // Raycast hacia adelante (dirección de movimiento) para detectar fin de suelo
            Vector2 groundRayOrigin = (Vector2)transform.position + movementDirection * EdgeCheckDistance;
            RaycastHit2D groundCheck = Physics2D.Raycast(groundRayOrigin, Vector2.down, EdgeRayLength, GroundLayer);

            // Raycast horizontal para detectar paredes
            Vector2 wallRayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            RaycastHit2D wallCheck = Physics2D.Raycast(wallRayOrigin, movementDirection, 0.7f, GroundLayer);

            // Calcular distancia recorrida desde el último giro
            float distanceTraveled = Mathf.Abs(transform.position.x - startPosition.x);

            // Si no hay suelo adelante O hay pared (raycast o colisión) O llegó a la distancia objetivo: empezar pausa
            if (!groundCheck || wallCheck || HitWall || distanceTraveled >= currentPatrolDistance)
            {
                previousDirection = movementDirection;
                IsWaiting = true;
                HitWall = false; // Reiniciar el flag
            }

            HandleFlip(movementDirection.x);
        }

        public void ChasePlayer()
        {
            currentSpeed = EnemyType.ChaseSpeed;

            if (Player == null)
            {
                movementDirection = Vector2.zero;
                return;
            }

            Vector2 direction = (Player.position - transform.position).normalized;
            movementDirection = new Vector2(direction.x, 0f);

            HandleFlip(movementDirection.x);
        }

        public void ApplyMovement()
        {
            if (movementDirection != Vector2.zero)
            {
                Rigidbody.linearVelocityX = movementDirection.x * currentSpeed;
            }
            else
            {
                Rigidbody.linearVelocityX = 0f;
            }
        }

        public void SetVelocityX(float velocityX)
        {
            Rigidbody.linearVelocityX = velocityX;
            movementDirection = Vector2.zero;
        }

        private void HandleFlip(float directionX)
        {
            if (directionX > 0.01f)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (directionX < -0.01f)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        public void PerformAttack()
        {
            if (Player == null || EnemyType == null) return;

            AudioManager.instance.Play("SkeletonSound");
            PlayerController directTarget = GetPlayerController(Player);
            if (CanHitPlayer(directTarget))
            {
                ApplyDamageToPlayer(directTarget);
                return;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, EnemyType.AttackRange, PlayerLayer);
            foreach (Collider2D hit in hits)
            {
                PlayerController playerController = GetPlayerController(hit.transform);
                if (CanHitPlayer(playerController))
                {
                    ApplyDamageToPlayer(playerController);
                    return;
                }
            }

        }

        private PlayerController GetPlayerController(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                return playerController;
            }

            return target.GetComponentInParent<PlayerController>();
        }

        private bool CanHitPlayer(PlayerController playerController)
        {
            if (playerController == null)
            {
                return false;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerController.transform.position);
            return distanceToPlayer <= EnemyType.AttackRange;
        }

        private void ApplyDamageToPlayer(PlayerController playerController)
        {
            Vector2 hitDirection = (playerController.transform.position - transform.position).normalized;
            if (hitDirection == Vector2.zero)
            {
                hitDirection = transform.right;
            }

            playerController.TakeDamage(EnemyType.AttackDamage, hitDirection, 0.5f);
        }

        public bool TakeDamage(int damage, Vector2 direction, float knockcack)
        {
            StateMachine.ChangeState(HitState);
            CurrentHealth -= damage;
            Rigidbody.AddForce(direction * knockcack);
            if (CurrentHealth <= 0)
                IsDead = true;
            damageEffects.PlayHitEffects();
            return true;

        }

        public void PlayAttackSound()
        {
            if (EnemyType.AttackSound != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(EnemyType.AttackSound);
            }
        }

        public void PlayHitSound()
        {
            if (EnemyType.HitSound != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(EnemyType.HitSound);
            }
        }

        public void PlayDeathSound()
        {
            if (EnemyType.DeathSound != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(EnemyType.DeathSound);
            }
        }

        public void DisableColliders()
        {
            foreach (var collider in Colliders)
            {
                collider.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (EnemyType != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, EnemyType.DetectionRange);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, EnemyType.AttackRange);
            }

            // Raycast de suelo (cian) — muestra el EdgeCheck en el editor
            Gizmos.color = Color.cyan;
            Vector3 groundRayOrigin = transform.position + (Vector3)movementDirection * EdgeCheckDistance;
            Gizmos.DrawLine(groundRayOrigin, groundRayOrigin + Vector3.down * EdgeRayLength);

            // Raycast de paredes (magenta)
            Gizmos.color = Color.magenta;
            Vector3 wallRayOrigin = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(wallRayOrigin, wallRayOrigin + (Vector3)movementDirection * 0.7f);

            // Distancia de patrulla (verde)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)movementDirection * currentPatrolDistance);
            Gizmos.DrawWireSphere(transform.position, EnemyType.AttackRange);

        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Si colisiona con algo en la capa Ground, comprobar si es una pared
            if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
            {
                // Iterar sobre todos los puntos de contacto
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // La normal del contacto apunta hacia afuera de la superficie colisionada.
                    // Si estamos moviéndonos hacia la derecha (1,0) y chocamos con una pared a la derecha,
                    // la normal de la pared será (-1,0). El producto punto será -1.
                    if (Vector2.Dot(contact.normal, movementDirection) < -0.5f)
                    {
                        HitWall = true;
                        break;
                    }
                }


            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // También comprobamos triggers por si acaso
            if (((1 << other.gameObject.layer) & GroundLayer) != 0)
            {
                HitWall = true;
            }
        }

    }
}

using UnityEngine;
using Enemies.DarkMagician.StateMachine;

namespace Enemies.DarkMagician
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    public class DarkMagicianController : MonoBehaviour, IDamageable
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Inspector
        // ─────────────────────────────────────────────────────────────────────────

        [Header("Enemy Type")]
        public EnemyType EnemyType;

        [Header("Patrol Settings")]
        public LayerMask GroundLayer;
        public float PatrolDistance1 = 2f;
        public float PatrolDistance2 = 3f;
        public float IdleWaitTime    = 1.2f;
        [Tooltip("Distancia desde el borde para el raycast de suelo")]
        public float EdgeCheckDistance = 0.4f;
        [Tooltip("Longitud del raycast hacia abajo para detectar borde")]
        public float EdgeRayLength = 1.5f;

        [Header("Player Detection")]
        public Transform Player;
        public LayerMask PlayerLayer;

        [Header("Projectile")]
        [Tooltip("Transform desde donde se dispara el proyectil (FirePoint)")]
        public Transform FirePoint;
        [Tooltip("Pool de proyectiles — arrastrar el objeto Pool de la escena")]
        public GameObject Projectile;

        // ─────────────────────────────────────────────────────────────────────────
        //  Componentes
        // ─────────────────────────────────────────────────────────────────────────

        public Rigidbody2D    Rigidbody     { get; private set; }
        public Animator       Animator      { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public AudioSource    AudioSource   { get; private set; }
        public Collider2D[]   Colliders     { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  State Machine
        // ─────────────────────────────────────────────────────────────────────────

        public StateMachine.StateMachine    StateMachine  { get; private set; }
        public DarkMagicianIdleState        IdleState     { get; private set; }
        public DarkMagicianWalkState        WalkState     { get; private set; }
        public DarkMagicianAttackState      AttackState   { get; private set; }
        public DarkMagicianDeadState        DeadState     { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Flags de estado
        // ─────────────────────────────────────────────────────────────────────────

        public bool IsDead         { get; private set; } = false;
        public bool PlayerDetected { get; private set; } = false;
        public bool IsInAttackRange { get; private set; } = false;
        public bool IsWaiting      = false;
        public bool HitWall        = false;

        // ─────────────────────────────────────────────────────────────────────────
        //  Stats
        // ─────────────────────────────────────────────────────────────────────────

        public float CurrentHealth { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Datos de movimiento internos
        // ─────────────────────────────────────────────────────────────────────────

        private Vector2 startPosition;
        private Vector2 movementDirection;
        private Vector2 previousDirection;
        private float   currentSpeed;
        private float   currentPatrolDistance;
        private bool    usingFirstDistance = true;
        private float   waitTimer          = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        //  Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            Rigidbody      = GetComponent<Rigidbody2D>();
            Animator       = GetComponent<Animator>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            AudioSource    = GetComponent<AudioSource>();
            Colliders      = GetComponents<Collider2D>();

            if (EnemyType != null)
                CurrentHealth = EnemyType.MaxHealth;

            startPosition        = transform.position;
            movementDirection    = Vector2.right;
            currentPatrolDistance = PatrolDistance1;

            StateMachine = new StateMachine.StateMachine();
            IdleState    = new DarkMagicianIdleState(this, StateMachine);
            WalkState    = new DarkMagicianWalkState(this, StateMachine);
            AttackState  = new DarkMagicianAttackState(this, StateMachine);
            DeadState    = new DarkMagicianDeadState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(WalkState);
        }

        private void Update()
        {
            if (IsDead && StateMachine.CurrentState != DeadState)
            {
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

        // ─────────────────────────────────────────────────────────────────────────
        //  Detección
        // ─────────────────────────────────────────────────────────────────────────

        private void CheckPlayerDetection()
        {
            if (Player == null || EnemyType == null) { PlayerDetected = false; return; }
            float dist = Vector2.Distance(transform.position, Player.position);
            PlayerDetected = dist <= EnemyType.DetectionRange;
        }

        private void CheckAttackRange()
        {
            if (Player == null || EnemyType == null) { IsInAttackRange = false; return; }
            float dist = Vector2.Distance(transform.position, Player.position);
            IsInAttackRange = dist <= EnemyType.AttackRange;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — Patrulla
        // ─────────────────────────────────────────────────────────────────────────

        public void Patrol()
        {
            if (IsWaiting)
            {
                waitTimer += Time.deltaTime;
                movementDirection = Vector2.zero;
                currentSpeed      = 0f;

                if (waitTimer >= IdleWaitTime)
                {
                    IsWaiting      = false;
                    waitTimer      = 0f;
                    movementDirection = -previousDirection;
                    startPosition  = transform.position;

                    usingFirstDistance    = !usingFirstDistance;
                    currentPatrolDistance = usingFirstDistance ? PatrolDistance1 : PatrolDistance2;
                }
                return;
            }

            currentSpeed = EnemyType.WalkSpeed;

            // Raycast de borde de plataforma
            Vector2 groundRayOrigin = (Vector2)transform.position + movementDirection * EdgeCheckDistance;
            RaycastHit2D groundCheck = Physics2D.Raycast(groundRayOrigin, Vector2.down, EdgeRayLength, GroundLayer);

            // Raycast de pared
            Vector2 wallRayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            RaycastHit2D wallCheck = Physics2D.Raycast(wallRayOrigin, movementDirection, 0.7f, GroundLayer);

            float distanceTraveled = Mathf.Abs(transform.position.x - startPosition.x);

            if (!groundCheck || wallCheck || HitWall || distanceTraveled >= currentPatrolDistance)
            {
                previousDirection = movementDirection;
                IsWaiting         = true;
                HitWall           = false;
            }

            HandleFlip(movementDirection.x);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — Persecución
        // ─────────────────────────────────────────────────────────────────────────

        public void ChasePlayer()
        {
            if (Player == null) { movementDirection = Vector2.zero; return; }
            currentSpeed = EnemyType.ChaseSpeed;
            Vector2 direction  = (Player.position - transform.position).normalized;
            movementDirection  = new Vector2(direction.x, 0f);
            HandleFlip(movementDirection.x);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — Aplicar física
        // ─────────────────────────────────────────────────────────────────────────

        public void ApplyMovement()
        {
            Rigidbody.linearVelocityX = movementDirection != Vector2.zero
                ? movementDirection.x * currentSpeed
                : 0f;
        }

        public void SetVelocityX(float vx)
        {
            Rigidbody.linearVelocityX = vx;
            movementDirection         = Vector2.zero;
        }

        private void HandleFlip(float directionX)
        {
            if      (directionX >  0.01f) transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (directionX < -0.01f) transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Combate — Proyectil
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Dispara un proyectil desde el FirePoint hacia el jugador.
        /// Puede llamarse también desde un Animation Event en el clip "Attack".
        /// </summary>
       
        public void FireProjectile()
        {
            if (FirePoint == null || Projectile == null) return;

            Vector2 dir = (Player.position - FirePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            FirePoint.rotation = Quaternion.Euler(0, 0, angle);
            
            GameObject proj = Instantiate(Projectile, FirePoint.position, Quaternion.identity);

            var rb = proj.GetComponent<Rigidbody2D>();
            var proyectileComponent = proj.GetComponent<EnemyProjectile>();

            rb.linearVelocity = dir * proyectileComponent.speed;

            if (proj == null) return;

        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Combate — Daño / Muerte
        // ─────────────────────────────────────────────────────────────────────────

        public bool TakeDamage(int damage, Vector2 direction, float knockcack)
        {
            CurrentHealth -= damage;
            Rigidbody.AddForce(direction * knockcack);
            if (CurrentHealth <= 0)
                IsDead = true;
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Audio
        // ─────────────────────────────────────────────────────────────────────────

        public void PlayAttackSound()
        {
            if (EnemyType?.AttackSound != null && AudioSource != null)
                AudioSource.PlayOneShot(EnemyType.AttackSound);
        }

        public void PlayDeathSound()
        {
            if (EnemyType?.DeathSound != null && AudioSource != null)
                AudioSource.PlayOneShot(EnemyType.DeathSound);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Colliders
        // ─────────────────────────────────────────────────────────────────────────

        public void DisableColliders()
        {
            foreach (var col in Colliders)
                col.enabled = false;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Colisiones
        // ─────────────────────────────────────────────────────────────────────────

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & GroundLayer) != 0)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (Vector2.Dot(contact.normal, movementDirection) < -0.5f)
                    {
                        HitWall = true;
                        break;
                    }
                }
            }

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Gizmos
        // ─────────────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (EnemyType != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, EnemyType.DetectionRange);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, EnemyType.AttackRange);
            }

            // Raycast de borde (cian)
            Gizmos.color = Color.cyan;
            Vector3 groundOrigin = transform.position + (Vector3)movementDirection * EdgeCheckDistance;
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector3.down * EdgeRayLength);

            // Raycast de pared (magenta)
            Gizmos.color = Color.magenta;
            Vector3 wallOrigin = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(wallOrigin, wallOrigin + (Vector3)movementDirection * 0.7f);
        }
    }
}

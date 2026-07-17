using UnityEngine;
using Enemies.Murcielago.StateMachine;
using UnityEngine.UIElements;

namespace Enemies.Murcielago
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    public class MurcielagoController : MonoBehaviour, IDamageable
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Inspector
        // ─────────────────────────────────────────────────────────────────────────

        [Header("Enemy Type")]
        public EnemyType EnemyType;

        [Header("Player")]
        public Transform Player;
        public LayerMask PlayerLayer;

        [Header("Patrol (Fly)")]
        [Tooltip("Distancia horizontal que recorre antes de girar")]
        public float PatrolDistance = 3f;
        [Tooltip("Amplitud de la oscilación vertical en zig-zag")]
        public float SineAmplitude = 1f;
        [Tooltip("Frecuencia de la oscilación vertical")]
        public float SineFrequency = 2f;

        [Header("Attack (Dive)")]
        [Tooltip("Duración en segundos del tramo de bajada hacia el jugador")]
        public float DiveDuration = 0.5f;
        [Tooltip("Duración en segundos del tramo de regreso al punto de inicio")]
        public float ReturnDuration = 0.8f;

        [Header("Effects")]
        [SerializeField] private DamageEffects damageEffects;

        // ─────────────────────────────────────────────────────────────────────────
        //  Componentes (acceso de solo lectura desde los estados)
        // ─────────────────────────────────────────────────────────────────────────

        public Rigidbody2D Rigidbody       { get; private set; }
        public Animator    Animator        { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public AudioSource AudioSource     { get; private set; }
        public Collider2D[] Colliders      { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  State Machine
        // ─────────────────────────────────────────────────────────────────────────

        public StateMachine.StateMachine StateMachine { get; private set; }
        public MurcielagoFlyState    FlyState    { get; private set; }
        public MurcielagoAttackState AttackState { get; private set; }
        public MurcielagoDeadState   DeadState   { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Flags de estado
        // ─────────────────────────────────────────────────────────────────────────

        public bool IsDead      { get; private set; } = false;
        public bool IsAttacking { get; private set; } = false;

        // ─────────────────────────────────────────────────────────────────────────
        //  Stats actuales
        // ─────────────────────────────────────────────────────────────────────────

        public float CurrentHealth { get; private set; }

        // ─────────────────────────────────────────────────────────────────────────
        //  Datos de patrulla interna
        // ─────────────────────────────────────────────────────────────────────────

        private Vector2 patrolStart;
        private float patrolDirection = 1f;   // +1 = derecha, -1 = izquierda
        private float lastSineY;              // Altura Y previa para calcular la velocidad

        // Punto objetivo del dive (capturado al iniciar el ataque)
        private Vector2 diveTarget;
        private Vector2 diveOrigin;

        // ─────────────────────────────────────────────────────────────────────────
        //  Unity lifecycle
        // ─────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            Rigidbody     = GetComponent<Rigidbody2D>();
            Animator      = GetComponent<Animator>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            AudioSource   = GetComponent<AudioSource>();
            Colliders     = GetComponents<Collider2D>();

            if (EnemyType != null)
                CurrentHealth = EnemyType.MaxHealth;

            patrolStart = transform.position;
            lastSineY   = transform.position.y;

            // Desactivar gravedad — el murciélago vuela
            Rigidbody.gravityScale = 0f;

            StateMachine = new StateMachine.StateMachine();
            FlyState     = new MurcielagoFlyState(this, StateMachine);
            AttackState  = new MurcielagoAttackState(this, StateMachine);
            DeadState    = new MurcielagoDeadState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(FlyState);
        }

        private void Update()
        {
            if (IsDead && StateMachine.CurrentState != DeadState)
            {
                DropItem();
                StateMachine.ChangeState(DeadState);
                return;
            }

            CheckAttackCondition();
            StateMachine.CurrentState.Update();
        }

        private void FixedUpdate()
        {
            StateMachine.CurrentState.FixedUpdate();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Detección
        // ─────────────────────────────────────────────────────────────────────────

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
                        EnemyType.Drops.Length + 1
                    );

                Instantiate(
                    EnemyType.Drops[index],
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        private void CheckAttackCondition()
        {
            if (IsDead || Player == null || EnemyType == null)
            {
                IsAttacking = false;
                return;
            }

            float dist = Vector2.Distance(transform.position, Player.position);
            // Solo activa el flag si está dentro del rango de detección y no está ya atacando
            IsAttacking = (dist <= EnemyType.DetectionRange) && (StateMachine.CurrentState == FlyState);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — Fly (zig-zag sinusoidal)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Desplazamiento horizontal con oscilación vertical sinusoidal.
        /// Llama a esto desde FixedUpdate del FlyState pasando el tiempo acumulado.
        /// </summary>
        public void ApplyFlyMovement(float sineTimer)
        {
            if (EnemyType == null) return;

            // ── Horizontal ────────────────────────────────────────────────────
            float speed = EnemyType.WalkSpeed;
            float newX  = Rigidbody.position.x + patrolDirection * speed * Time.fixedDeltaTime;

            // Comprobar si llegó al límite del recorrido
            float traveled = Mathf.Abs(newX - patrolStart.x);
            if (traveled >= PatrolDistance)
            {
                patrolDirection = -patrolDirection;
                patrolStart     = Rigidbody.position;
            }

            // ── Vertical (sinusoidal) ─────────────────────────────────────────
            float newY = patrolStart.y + Mathf.Sin(sineTimer * SineFrequency) * SineAmplitude;

            // ── Aplicar posición ──────────────────────────────────────────────
            Vector2 newPos = new Vector2(newX, newY);
            Rigidbody.MovePosition(newPos);

            // ── Flip de sprite ────────────────────────────────────────────────
            HandleFlip(patrolDirection);

            lastSineY = newY;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — Attack (dive-bomb en arco)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Captura el punto objetivo del dive justo antes de que AttackState entre.
        /// Se llama internamente al hacer Enter en AttackState.
        /// </summary>
        public void CaptureDiveTarget()
        {
            diveOrigin = transform.position;
            diveTarget = Player != null ? (Vector2)Player.position : (Vector2)transform.position;
        }

        /// <summary>
        /// Mueve al murciélago en arco hacia el jugador (fase de bajada).
        /// Devuelve true cuando alcanza el objetivo.
        /// </summary>
        public bool ApplyDiveToPlayer(float t)
        {
            float progress = Mathf.Clamp01(t / DiveDuration);

            // Interpolación con un arco (curva cúbica): va hacia abajo y luego llega
            // Usamos una curva senoidal para dar la forma de arco de la imagen
            float easedProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);

            Vector2 newPos = Vector2.Lerp(diveOrigin, diveTarget, easedProgress);
            Rigidbody.MovePosition(newPos);

            HandleFlip(diveTarget.x - diveOrigin.x);

            return progress >= 1f;
        }

        /// <summary>
        /// Mueve al murciélago de regreso al punto de inicio del ataque.
        /// Devuelve true cuando llega.
        /// </summary>
        public bool ApplyReturnToStart(Vector2 startPos, float t)
        {
            float progress = Mathf.Clamp01(t / ReturnDuration);
            float easedProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);

            Vector2 newPos = Vector2.Lerp(diveTarget, startPos, easedProgress);
            Rigidbody.MovePosition(newPos);

            HandleFlip(startPos.x - diveTarget.x);

            // Actualizar el punto de patrulla al regresar
            if (progress >= 1f)
                patrolStart = startPos;

            return progress >= 1f;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Movimiento — utilidades
        // ─────────────────────────────────────────────────────────────────────────

        public void StopMovement()
        {
            Rigidbody.linearVelocity = Vector2.zero;
        }

        private void HandleFlip(float directionX)
        {
            if (directionX > 0.01f)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (directionX < -0.01f)
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Combate
        // ─────────────────────────────────────────────────────────────────────────

        public void PerformAttack()
        {
            if (Player == null || EnemyType == null) return;

            AudioManager.instance.Play("BatSound");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, EnemyType.AttackRange, PlayerLayer);
            foreach (Collider2D hit in hits)
            {
                hit.GetComponent<PlayerController>()?.TakeDamage(EnemyType.AttackDamage, Rigidbody.linearVelocity, 0.5f);
            }
        }

        public bool TakeDamage(int damage, Vector2 direction, float knockcack)
        {
            CurrentHealth -= damage;
            Rigidbody.AddForce(direction * knockcack);
            if (CurrentHealth <= 0)
                IsDead = true;
            damageEffects.PlayHitEffects();
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
        //  Gizmos (editor)
        // ─────────────────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (EnemyType != null)
            {
                // Rango de detección (amarillo)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, EnemyType.DetectionRange);

                // Rango de ataque (rojo)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, EnemyType.AttackRange);
            }

            // Recorrido de patrulla (verde)
            Gizmos.color = Color.green;
            Vector3 left  = patrolStart + Vector2.left  * PatrolDistance;
            Vector3 right = patrolStart + Vector2.right * PatrolDistance;
            Gizmos.DrawLine(left, right);
        }
    }
}

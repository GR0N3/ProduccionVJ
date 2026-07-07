using UnityEngine;

namespace Enemies.DarkMagician.StateMachine
{
    /// <summary>
    /// Estado Attack: el Dark Magician se detiene, reproduce la animación de ataque
    /// y lanza un proyectil hacia el jugador usando el Object Pool.
    /// 
    /// El proyectil se dispara a mitad de la animación mediante un Animation Event
    /// que llama a magician.FireProjectile(), o automáticamente pasado AttackCooldown.
    /// 
    /// Transiciones:
    ///   → WalkState si el jugador sale del rango de ataque
    ///   → DeadState si IsDead es true
    /// </summary>
    public class DarkMagicianAttackState : DarkMagicianState
    {
        private float attackTimer;
        private bool hasShot;

        public DarkMagicianAttackState(DarkMagicianController magician, StateMachine stateMachine)
            : base(magician, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            magician.Animator.Play("Attack");
            magician.SetVelocityX(0f);
            attackTimer = 0f;
            hasShot     = false;
            magician.PlayAttackSound();
        }

        public override void Update()
        {
            base.Update();

            if (magician.IsDead)
            {
                stateMachine.ChangeState(magician.DeadState);
                return;
            }

            attackTimer += Time.deltaTime;

            // Disparar el proyectil a la mitad del cooldown (si no usa Animation Event)
            if (!hasShot && attackTimer >= magician.EnemyType.AttackCooldown * 0.4f)
            {
                hasShot = true;
                //magician.FireProjectile();
            }

            // Cuando expira el cooldown → decidir próximo estado
            if (attackTimer >= magician.EnemyType.AttackCooldown)
            {
                if (magician.IsInAttackRange && magician.PlayerDetected)
                {
                    // Volver a atacar: reiniciar el estado
                    stateMachine.ChangeState(magician.AttackState);
                }
                else
                {
                    stateMachine.ChangeState(magician.WalkState);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}

using UnityEngine;

namespace Enemies.DarkMagician.StateMachine
{
    /// <summary>
    /// Estado Idle: el Dark Magician se detiene y reproduce la animación de reposo.
    /// Transiciones:
    ///   → WalkState   si el jugador NO está en rango de detección
    ///   → AttackState si el jugador está en rango de ataque
    ///   → DeadState   si IsDead es true
    /// </summary>
    public class DarkMagicianIdleState : DarkMagicianState
    {
        private float waitTimer = 0;

        public DarkMagicianIdleState(DarkMagicianController magician, StateMachine stateMachine)
            : base(magician, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            magician.Animator.Play("Idle");
            magician.SetVelocityX(0f);
            waitTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (magician.IsDead)
            {
                stateMachine.ChangeState(magician.DeadState);
                return;
            }

            // Si el jugador entra en rango de ataque → atacar
            if (magician.PlayerDetected && magician.IsInAttackRange)
            {
                stateMachine.ChangeState(magician.AttackState);
                return;
            }

            // Si el jugador está detectado pero fuera de rango de ataque → perseguir caminando
            if (magician.PlayerDetected && !magician.IsInAttackRange)
            {
                stateMachine.ChangeState(magician.WalkState);
                return;
            }

            // Sin jugador: patrullar
            magician.Patrol();
            if (!magician.IsWaiting)
            {
                stateMachine.ChangeState(magician.WalkState);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            magician.ApplyMovement();
        }
    }
}

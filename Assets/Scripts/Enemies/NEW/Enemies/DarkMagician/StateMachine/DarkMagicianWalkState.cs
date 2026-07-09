using UnityEngine;

namespace Enemies.DarkMagician.StateMachine
{
    /// <summary>
    /// Estado Walk: el Dark Magician patrulla o persigue al jugador caminando.
    /// Transiciones:
    ///   → IdleState   al llegar al límite de patrulla o chocar con pared
    ///   → AttackState si el jugador está en rango de ataque
    ///   → DeadState   si IsDead es true
    /// </summary>
    public class DarkMagicianWalkState : DarkMagicianState
    {
        public DarkMagicianWalkState(DarkMagicianController magician, StateMachine stateMachine)
            : base(magician, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            magician.Animator.Play("Walk");
        }

        public override void Update()
        {
            base.Update();

            if (magician.IsDead)
            {
                stateMachine.ChangeState(magician.DeadState);
                return;
            }

            if (magician.PlayerDetected)
            {
                // Si está en rango de ataque → atacar
                if (magician.IsInAttackRange)
                {
                    stateMachine.ChangeState(magician.AttackState);
                    return;
                }
                // Si detecta pero no en rango → perseguir (chase)
                magician.ChasePlayer();
            }
            else
            {
                // Sin jugador → patrullar
                magician.Patrol();

                // Al pausar en la patrulla → pasar a Idle
                if (magician.IsWaiting)
                {
                    stateMachine.ChangeState(magician.IdleState);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            magician.ApplyMovement();
        }
    }
}

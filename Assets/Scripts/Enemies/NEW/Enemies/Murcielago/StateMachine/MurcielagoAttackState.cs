using UnityEngine;

namespace Enemies.Murcielago.StateMachine
{
    /// <summary>
    /// Estado de ataque: el murciélago realiza un dive-bomb en arco hacia el jugador.
    /// Se lanza desde arriba en una curva hacia el jugador y regresa a su posición de inicio.
    /// Vuelve a FlyState cuando termina el ataque o pierde al jugador.
    /// Si muere, transiciona a DeadState.
    /// </summary>
    public class MurcielagoAttackState : MurcielagoState
    {
        private Vector2 startPosition;
        private float attackTimer;

        // Fase 0 = lanzándose hacia el jugador, Fase 1 = regresando a start
        private int attackPhase;

        public MurcielagoAttackState(MurcielagoController murcielago, StateMachine stateMachine)
            : base(murcielago, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            murcielago.Animator.Play("Attack");
            startPosition = murcielago.transform.position;
            attackTimer   = 0f;
            attackPhase   = 0;
            // Capturar la posición del jugador en el momento exacto del ataque
            murcielago.CaptureDiveTarget();
            murcielago.PlayAttackSound();
        }

        public override void Update()
        {
            base.Update();

            if (murcielago.IsDead)
            {
                stateMachine.ChangeState(murcielago.DeadState);
                return;
            }

            attackTimer += Time.deltaTime;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (attackPhase == 0)
            {
                // Lanzarse en arco hacia el jugador
                bool reached = murcielago.ApplyDiveToPlayer(attackTimer);
                if (reached)
                {
                    murcielago.PerformAttack();
                    attackPhase = 1;
                    attackTimer = 0f;
                }
            }
            else
            {
                // Regresar a la posición de inicio
                bool returned = murcielago.ApplyReturnToStart(startPosition, attackTimer);
                if (returned)
                {
                    stateMachine.ChangeState(murcielago.FlyState);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}

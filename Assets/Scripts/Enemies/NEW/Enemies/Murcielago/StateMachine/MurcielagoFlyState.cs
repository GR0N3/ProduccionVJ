using UnityEngine;

namespace Enemies.Murcielago.StateMachine
{
    /// <summary>
    /// Estado de vuelo en zig-zag sinusoidal.
    /// El murciélago patrulla moviéndose horizontalmente mientras oscila verticalmente
    /// (movimiento en onda senoidal), girando al llegar a los puntos de patrulla.
    /// Si detecta al jugador en rango, transiciona a AttackState.
    /// Si muere, transiciona a DeadState.
    /// </summary>
    public class MurcielagoFlyState : MurcielagoState
    {
        private float sineTimer;

        public MurcielagoFlyState(MurcielagoController murcielago, StateMachine stateMachine)
            : base(murcielago, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            murcielago.Animator.Play("Flying");
            sineTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (murcielago.IsDead)
            {
                stateMachine.ChangeState(murcielago.DeadState);
                return;
            }

            if (murcielago.IsAttacking)
            {
                stateMachine.ChangeState(murcielago.AttackState);
                return;
            }

            // Acumular tiempo para el movimiento sinusoidal
            sineTimer += Time.deltaTime;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            murcielago.ApplyFlyMovement(sineTimer);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}

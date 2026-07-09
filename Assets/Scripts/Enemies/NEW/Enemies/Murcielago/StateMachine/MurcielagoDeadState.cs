using UnityEngine;

namespace Enemies.Murcielago.StateMachine
{
    /// <summary>
    /// Estado de muerte: reproduce la animación de muerte, detiene el movimiento,
    /// deshabilita los colliders y reproduce el sonido de muerte.
    /// Estado terminal — no hay transiciones de salida.
    /// </summary>
    public class MurcielagoDeadState : MurcielagoState
    {
        public MurcielagoDeadState(MurcielagoController murcielago, StateMachine stateMachine)
            : base(murcielago, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            murcielago.Animator.Play("Dead");
            murcielago.StopMovement();
            murcielago.PlayDeathSound();
            murcielago.DisableColliders();
        }

        public override void Update()
        {
            base.Update();
            // Estado terminal — sin transiciones
        }
    }
}

namespace Enemies.DarkMagician.StateMachine
{
    /// <summary>
    /// Estado Dead: estado terminal.
    /// Reproduce la animación de muerte, detiene el movimiento,
    /// desactiva los colliders y reproduce el sonido de muerte.
    /// No hay transiciones de salida desde este estado.
    /// </summary>
    public class DarkMagicianDeadState : DarkMagicianState
    {
        public DarkMagicianDeadState(DarkMagicianController magician, StateMachine stateMachine)
            : base(magician, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            magician.Animator.Play("Dead");
            magician.SetVelocityX(0f);
            magician.PlayDeathSound();
            magician.DisableColliders();
        }

        public override void Update()
        {
            base.Update();
            // Estado terminal — sin transiciones
        }
    }
}

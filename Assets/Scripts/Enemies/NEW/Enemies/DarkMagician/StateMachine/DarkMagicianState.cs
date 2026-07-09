namespace Enemies.DarkMagician.StateMachine
{
    /// <summary>
    /// Clase abstracta base para todos los estados del Dark Magician.
    /// Guarda referencia al controlador y a la state machine.
    /// </summary>
    public abstract class DarkMagicianState : IState
    {
        protected DarkMagicianController magician;
        protected StateMachine stateMachine;

        public DarkMagicianState(DarkMagicianController magician, StateMachine stateMachine)
        {
            this.magician     = magician;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter()       { }
        public virtual void Update()      { }
        public virtual void FixedUpdate() { }
        public virtual void Exit()        { }
    }
}

namespace Enemies.Murcielago.StateMachine
{
    /// <summary>
    /// Clase base abstracta para todos los estados del Murciélago.
    /// Cada estado recibe referencia al controlador y a la state machine.
    /// </summary>
    public abstract class MurcielagoState : IState
    {
        protected MurcielagoController murcielago;
        protected StateMachine stateMachine;

        public MurcielagoState(MurcielagoController murcielago, StateMachine stateMachine)
        {
            this.murcielago = murcielago;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}

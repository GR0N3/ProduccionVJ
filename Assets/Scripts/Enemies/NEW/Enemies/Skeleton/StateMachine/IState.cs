namespace Enemies.Skeleton.StateMachine
{
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}

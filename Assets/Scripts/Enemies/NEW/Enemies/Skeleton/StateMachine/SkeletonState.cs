using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public abstract class SkeletonState : IState
    {
        protected SkeletonController skeleton;
        protected StateMachine stateMachine;

        public SkeletonState(SkeletonController skeleton, StateMachine stateMachine)
        {
            this.skeleton = skeleton;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}

using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public class SkeletonDeadState : SkeletonState
    {
        public SkeletonDeadState(SkeletonController skeleton, StateMachine stateMachine) : base(skeleton, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            skeleton.Animator.Play("Dead");
            skeleton.SetVelocityX(0f);
            skeleton.PlayDeathSound();
            skeleton.DisableColliders();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}

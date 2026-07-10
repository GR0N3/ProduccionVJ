using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public class SkeletonHitState : SkeletonState
    {
        private float hitTimer;
        private const float HitDuration = 0.1f;

        public SkeletonHitState(SkeletonController skeleton, StateMachine stateMachine) : base(skeleton, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            skeleton.Animator.Play("Hit");
            skeleton.SetVelocityX(0f);
            hitTimer = 0f;
            skeleton.PlayHitSound();
        }

        public override void Update()
        {
            base.Update();

            if (skeleton.IsDead)
            {
                stateMachine.ChangeState(skeleton.DeadState);
                return;
            }

            hitTimer += Time.deltaTime;

            if (hitTimer >= HitDuration)
            {
                skeleton.IsHit = false;
                stateMachine.ChangeState(skeleton.IdleState);
            }
        }
    }
}

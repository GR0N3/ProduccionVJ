using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public class SkeletonIdleState : SkeletonState
    {
        public SkeletonIdleState(SkeletonController skeleton, StateMachine stateMachine) : base(skeleton, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            skeleton.Animator.Play("Idle");
            skeleton.SetVelocityX(0f);
        }

        public override void Update()
        {
            base.Update();

            if (skeleton.IsDead)
            {
                stateMachine.ChangeState(skeleton.DeadState);
                return;
            }

            if (skeleton.IsHit)
            {
                stateMachine.ChangeState(skeleton.HitState);
                return;
            }

            if (skeleton.PlayerDetected)
            {
                if (skeleton.IsInAttackRange)
                {
                    stateMachine.ChangeState(skeleton.AttackState);
                }
                else
                {
                    stateMachine.ChangeState(skeleton.WalkState);
                }
                return;
            }

            // Update the wait timer logic in Patrol
            skeleton.Patrol();

            // Si ya terminó la espera, volver a caminar
            if (!skeleton.IsWaiting)
            {
                stateMachine.ChangeState(skeleton.WalkState);
            }
        }
    }
}

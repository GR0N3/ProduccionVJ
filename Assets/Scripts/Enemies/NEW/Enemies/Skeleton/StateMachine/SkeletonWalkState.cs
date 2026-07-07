using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public class SkeletonWalkState : SkeletonState
    {
        public SkeletonWalkState(SkeletonController skeleton, StateMachine stateMachine) : base(skeleton, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            skeleton.Animator.Play("Walk");
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
                    return;
                }
                else
                {
                    skeleton.ChasePlayer();
                    return;
                }
            }

            skeleton.Patrol();

            // Si está esperando, cambiar a Idle
            if (skeleton.IsWaiting)
            {
                stateMachine.ChangeState(skeleton.IdleState);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            skeleton.ApplyMovement();
        }
    }
}

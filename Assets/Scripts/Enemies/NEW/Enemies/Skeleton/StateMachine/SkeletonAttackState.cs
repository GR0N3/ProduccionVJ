using UnityEngine;

namespace Enemies.Skeleton.StateMachine
{
    public class SkeletonAttackState : SkeletonState
    {
        private float attackTimer;
        private bool hasAttacked;

        public SkeletonAttackState(SkeletonController skeleton, StateMachine stateMachine) : base(skeleton, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            skeleton.Animator.Play("Attack");
            skeleton.SetVelocityX(0f);
            attackTimer = 0f;
            hasAttacked = false;
            skeleton.PlayAttackSound();
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

            attackTimer += Time.deltaTime;

            if (attackTimer >= skeleton.EnemyType.AttackCooldown)
            {
                if (skeleton.PlayerDetected)
                {
                    if (skeleton.IsInAttackRange)
                    {
                        if (!hasAttacked)
                        {
                            hasAttacked = true;
                            skeleton.PerformAttack();
                        }
                    }
                    else
                    {
                        stateMachine.ChangeState(skeleton.WalkState);
                    }
                }
                else
                {
                    stateMachine.ChangeState(skeleton.WalkState);
                }
            }
        }
    }
}

using UnityEngine;

// ESTRATEGIA CONCRETA: PERSECUCIÓN
public class ChaseStrategy : IEnemyStrategy
{
    public void Execute(EnemyAI context)
    {
        if (context.player == null) return;

        float direction = context.player.position.x - context.transform.position.x;
        float moveDir = direction > 0 ? 1f : -1f;

        context.rb.linearVelocity = new Vector2(moveDir * context.chaseSpeed, context.rb.linearVelocity.y);

        if ((moveDir > 0 && !context.movingRight) || (moveDir < 0 && context.movingRight))
        {
            context.Flip();
        }
    }
}

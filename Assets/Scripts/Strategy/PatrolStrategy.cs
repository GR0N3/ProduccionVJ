using UnityEngine;

// ESTRATEGIA CONCRETA: PATRULLA
public class PatrolStrategy : IEnemyStrategy
{
    public void Execute(EnemyAI context)
    {
        // Movimiento de patrulla
        float moveDir = context.movingRight ? 1f : -1f;
        context.rb.linearVelocity = new Vector2(moveDir * context.patrolSpeed, context.rb.linearVelocity.y);

        // Si giramos hace muy poco, no volvemos a girar
        if (Time.time < context.lastTurnTime + context.turnCooldown) return;

        // Detección de bordes y paredes
        Vector2 sensorPos = context.sensorOrigin != null ? (Vector2)context.sensorOrigin.position : (Vector2)context.transform.position;
        RaycastHit2D groundInfo = Physics2D.Raycast(sensorPos, Vector2.down, context.groundCheckDistance, context.groundLayer);
        RaycastHit2D wallInfo = Physics2D.Raycast(sensorPos, Vector2.right * moveDir, context.wallCheckDistance, context.groundLayer);

        // Distancia de patrulla
        float distanceFromStart = context.transform.position.x - context.startPosition.x;
        bool outOfRange = (context.movingRight && distanceFromStart >= context.patrolRange) || (!context.movingRight && distanceFromStart <= -context.patrolRange);

        if (groundInfo.collider == null || wallInfo.collider != null || outOfRange)
        {
            context.Flip();
            context.lastTurnTime = Time.time;
        }
    }
}

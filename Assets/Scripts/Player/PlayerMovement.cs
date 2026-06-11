using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    private float speed;
    private float jumpForce;
    private float jumpCutMultiplier;

    [Header("Border")]
    private LayerMask borderLayer;

    [Header("Ground Check")]
    private float groundCheckDistance = 1.8f;
    private LayerMask groundLayer;

    private float acceleration = 100f;
    private float deceleration = 120f;

    public Transform CurrentPosition => rb.transform;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;
    bool isInLeft;

    // Aquí se guardará la posición del último checkpoint que tocaste
    private Vector2 lastSafePosition;
    private Transform playerTransform;
    private Vector3 originalScale;

    public void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = player.InputActions;
        groundLayer = player.GroundLayer;
        speed = player.speed;
        jumpForce = player.jumpForce;
        borderLayer = player.BorderLayer;
        jumpCutMultiplier = player.jumpCutMultiplier;

        // Al empezar el nivel, tu primer checkpoint por defecto es donde spawneas
        lastSafePosition = rb.position;

        playerTransform = player.transform;
        originalScale = playerTransform.localScale;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        Vector2 finalForce = direction.normalized * force;
        rb.AddForce(finalForce, ForceMode2D.Impulse);
    }

    public void Tick()
    {
        GroundCheck();
        LimitLeft();
        FlipSprite();
    }

    public void FixedTick()
    {
        float targetSpeed = movement.x * speed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float newVelocityX = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            accelRate * Time.fixedDeltaTime
        );
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;

        // REMOVIDO: Ya no guardamos la posición aquí automáticamente
    }

    void LimitLeft()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.left, 1f, borderLayer);
        isInLeft = hit.collider != null;

        if (isInLeft)
        {
            if (rb.linearVelocity.x < 0)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    // NUEVO: Esta función será llamada solo cuando toques un Checkpoint físico
    public void UpdateCheckpoint(Vector2 newCheckpointPos)
    {
        lastSafePosition = newCheckpointPos;
    }

    public void RespawnAtSafePosition()
    {
        // Te teletransporta exactamente a la posición del checkpoint (un poquito levantado para no trabarte)
        rb.position = lastSafePosition + new Vector2(0f, 0.5f);
        rb.linearVelocity = Vector2.zero;
    }

    private void FlipSprite()
    {
        if (rb.linearVelocity.x > 0.1f)
        {
            playerTransform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            playerTransform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }
}
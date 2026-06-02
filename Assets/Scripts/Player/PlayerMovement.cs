using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    private float speed;
    private float jumpForce;

    [Header("Border")]
    private LayerMask borderLayer;

    [Header("Ground Check")]
    private float groundCheckDistance = 1.2f;
    private LayerMask groundLayer;

    // Valores de aceleración para que no sea un tanque
    private float acceleration = 100f;
    private float deceleration = 120f;

    public Transform CurrentPosition => rb.transform;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;

    bool isInLeft;

    public void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = player.InputActions;
        groundLayer = player.GroundLayer;
        speed = player.speed;
        jumpForce = player.jumpForce;
        borderLayer = player.BorderLayer;
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

    public void ApplyKnockback(Vector2 direction, float force)
    {
        Vector2 finalForce = direction.normalized * force;
        rb.AddForce(finalForce, ForceMode2D.Impulse);
    }

    public void Tick()
    {
        GroundCheck();
        LimitLeft();
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
}
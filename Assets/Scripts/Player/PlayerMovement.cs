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
    private float groundCheckDistance = 1.2f;
    private LayerMask groundLayer;

    private float footOffset = 0.4f;
    private float acceleration = 100f;
    private float deceleration = 120f;

    public Transform CurrentPosition => rb.transform;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;
    bool isInLeft;
    private Vector2 lastSafePosition;

    private Animator animator;
    private Transform playerTransform;
    private string currentState;

    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_RUN = "Player_Run";
    const string PLAYER_JUMP = "Player_Jump";
    const string PLAYER_FALL = "Player_Fall";

    public void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = player.InputActions;
        groundLayer = player.GroundLayer;
        speed = player.speed;
        jumpForce = player.jumpForce;
        borderLayer = player.BorderLayer;
        jumpCutMultiplier = player.jumpCutMultiplier;

        lastSafePosition = rb.position;

        animator = player.animator;
        playerTransform = player.transform;
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
        UpdateAnimations();
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
        Vector2 center = rb.position;
        Vector2 left = rb.position + new Vector2(-footOffset, 0);
        Vector2 right = rb.position + new Vector2(footOffset, 0);

        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer);

        isGrounded = hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null;
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

    public void SetRespawnPoint(Vector2 newPoint)
    {
        lastSafePosition = newPoint;
    }

    public void RespawnAtSafePosition()
    {
        rb.position = lastSafePosition + new Vector2(0f, 0.5f);
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        if (rb.linearVelocity.x > 0.1f) playerTransform.localScale = new Vector3(1, 1, 1);
        else if (rb.linearVelocity.x < -0.1f) playerTransform.localScale = new Vector3(-1, 1, 1);

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f) ChangeAnimationState(PLAYER_JUMP);
            else ChangeAnimationState(PLAYER_FALL);
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) ChangeAnimationState(PLAYER_RUN);
            else ChangeAnimationState(PLAYER_IDLE);
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }
}
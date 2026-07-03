using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    private float speed;
    private float jumpForce;

    private LayerMask borderLayer;
    private Transform leftBorder;

    private float groundCheckDistance = 1.2f;
    private LayerMask groundLayer;
    private float acceleration;
    private float deceleration;

    public Transform CurrentPosition => rb.transform;

    private Collider2D collider;
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;

    private bool isInLeft;

    private AnimatorBrain anim = new();

    public void Init(PlayerController player)
    {
        collider = player.col;
        rb = player.rb;
        groundLayer = player.GroundLayer;
        speed = player.Speed;
        jumpForce = player.JumpForce;
        borderLayer = player.BorderLayer;
        acceleration = player.Acceleration;
        deceleration = player.Deceleration;
        anim = player.AnimatorBrain;
    }

    public void SetMoveInput(Vector2 input)
    {
        movement = input;

        FlipSprite(movement.x);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)                                                    
    {                                                                                                       
        SetMoveInput(ctx.ReadValue<Vector2>());                                                                
    }                                                                                                       
                                                                                                            
    public void OnJump(InputAction.CallbackContext ctx)                                                    
    {                                                                                                       
        Jump();
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
                                                                                          
        Debug.Log(isInLeft);
                                                                                          
    }
    private void GroundCheck()
    {
        Vector2 origin = new Vector2(collider.bounds.center.x, collider.bounds.min.y);

        isGrounded = Physics2D.OverlapBox(origin, new Vector2(collider.bounds.size.x, 0.1f), 0f, groundLayer);

        DrawBox(origin);
    }
    private void LimitLeft()
    {

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.left, 2, borderLayer);

        isInLeft = hit;

        if (isInLeft)
        {
            if (rb.position.x < hit.transform.position.x)
            {
                rb.position = new Vector2(leftBorder.position.x, rb.position.y);

                if (rb.linearVelocity.x < 0)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }

        }
        Debug.DrawRay(rb.position, Vector2.left * 2);
    }

    private void UpdateAnimations()
    {
        if (anim.IsLocked())
            return;

        // Aire
        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                anim.Play(PlayerAnimations.Jump);
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                anim.Play(PlayerAnimations.Fall);
            }

            return;
        }

        // Suelo
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            anim.Play(PlayerAnimations.Run);
        }
        else
        {
            anim.Play(PlayerAnimations.Idle);
        }
    }


    private void FlipSprite(float xInput)
    {
        var scale = rb.transform.localScale;

        if (xInput > 0)
        {
            rb.transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
        }
        else if (xInput < 0)
        {
            rb.transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
        }
    }

    private void DrawBox(Vector2 origin)
    {
        Vector2 center = origin;
        Vector2 size = new Vector2(collider.bounds.size.x * 0.9f, 0.1f);

        Vector2 half = size * 0.5f;

        Vector2 topLeft = center + new Vector2(-half.x, half.y);
        Vector2 topRight = center + new Vector2(half.x, half.y);
        Vector2 bottomLeft = center + new Vector2(-half.x, -half.y);
        Vector2 bottomRight = center + new Vector2(half.x, -half.y);

        Debug.DrawLine(topLeft, topRight, Color.green);
        Debug.DrawLine(topRight, bottomRight, Color.green);
        Debug.DrawLine(bottomRight, bottomLeft, Color.green);
        Debug.DrawLine(bottomLeft, topLeft, Color.green);
    }

    #region Upgrades
    public void UpgradeJump(float result) {jumpForce = result;}
    public void UpgradeSpeed(float result) {speed = result;}
    public void UpgradeAcceleration(float result) { acceleration = result; }
    public void UpgradeDeceleration(float result) { deceleration = result; }

    #endregion
}

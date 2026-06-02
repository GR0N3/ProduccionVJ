using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    private float speed;
    private float jumpForce;

    [Header("Border")]
    private LayerMask borderLayer;
    private Transform leftBorder;

    [Header("Ground Check")]
    private float groundCheckDistance = 1.2f;
    private LayerMask groundLayer;
    private float acceleration = 20f;
    private float deceleration = 25f;

    public Transform CurrentPosition => rb.transform;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;

    bool isInLeft;

    public void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = new InputSystem_Actions();
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
                                                                                          
        Debug.Log(isInLeft);
                                                                                          
    }

    public void UpgradeJump(float multiplier)
    {
        jumpForce *= multiplier;
    }

    public void UpgradeSpeed(float multiplier)
    {
        speed *= multiplier;
    }

    private void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider;

        Debug.DrawRay(rb.position,Vector2.down * groundCheckDistance,Color.yellow);
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

}
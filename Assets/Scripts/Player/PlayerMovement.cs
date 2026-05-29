using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    public float speed = 6f;
    public float jumpForce = 10f;

    [Header("Border")]
    private Transform leftBorder;

    [Header("Ground Check")]
    private float groundCheckDistance = 1.2f;
    private LayerMask groundLayer;
    private float acceleration = 20f;
    private float deceleration = 25f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;


    public void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = new InputSystem_Actions();
        leftBorder = player.LeftBorder;
        groundLayer = player.GroundLayer;
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
        Debug.Log(isGrounded);
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
                                                                                          
        LimitLeft();                                                                      
                                                                                          
    }

    void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.Log(hit.collider);
        isGrounded = hit.collider;

        Debug.DrawRay(rb.position,Vector2.down * groundCheckDistance,Color.yellow);
    }
    //Cambiar por un raycast
    void LimitLeft()
    {
        if (rb.position.x < leftBorder.position.x)
        {
            rb.position = new Vector2(leftBorder.position.x, rb.position.y);

            if (rb.linearVelocity.x < 0)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

}
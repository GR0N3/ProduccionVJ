using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float jumpForce = 10f;

    [Header("Border")]
    [SerializeField] private Transform leftBorder;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded;

    private InputSystem_Actions inputActions;


    void Init(PlayerController player)
    {
        rb = player.rb;
        inputActions = new InputSystem_Actions();
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }
    private void OnEnable()                                                                                 //Monobehauvior
    {                                                                                                       //Monobehauvior
        inputActions.Enable();                                                                              //Monobehauvior
                                                                                                            //Monobehauvior
        inputActions.Player.Move.performed += OnMove;                                                       //Monobehauvior
        inputActions.Player.Move.canceled += OnMove;                                                        //Monobehauvior
                                                                                                            //Monobehauvior
                                                                                                            //Monobehauvior
        inputActions.Player.Jump.performed += OnJump;                                                       //Monobehauvior
    }                                                                                                       //Monobehauvior
                                                                                                            //Monobehauvior
    private void OnDisable()                                                                                //Monobehauvior
    {                                                                                                       //Monobehauvior
        inputActions.Player.Move.performed -= OnMove;                                                       //Monobehauvior
        inputActions.Player.Move.canceled -= OnMove;                                                        //Monobehauvior
                                                                                                            //Monobehauvior
        inputActions.Player.Jump.performed -= OnJump;                                                       //Monobehauvior
                                                                                                            //Monobehauvior
        inputActions.Disable();                                                                             //Monobehauvior
    }                                                                                                       //Monobehauvior

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

    void Update()                                                                               //Monobehauvior
    {                                                                                           //Monobehauvior
        GroundCheck();
    }                                                                                           //Monobehauvior
                                                                                                //Monobehauvior
    void FixedUpdate()                                                                          //Monobehauvior
    {                                                                                           //Monobehauvior
        float targetSpeed = movement.x * speed;                                                 //Monobehauvior
                                                                                                //Monobehauvior
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;       //Monobehauvior
                                                                                                //Monobehauvior
        float newVelocityX = Mathf.MoveTowards(                                                 //Monobehauvior
            rb.linearVelocity.x,                                                                //Monobehauvior
            targetSpeed,                                                                        //Monobehauvior
            accelRate * Time.fixedDeltaTime                                                     //Monobehauvior
        );                                                                                      //Monobehauvior
                                                                                                //Monobehauvior
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);                     //Monobehauvior
                                                                                                //Monobehauvior
        LimitLeft();                                                                            //Monobehauvior
                                                                                                //Monobehauvior
    }

    void GroundCheck()
    {
        var ray = new Ray2D(transform.position, -transform.up);
        if (Physics2D.Raycast(ray.origin,ray.direction, groundCheckDistance + 1, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
            Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
    }

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




    void OnDrawGizmosSelected()                                                                 //Monobehauvior
    {                                                                                           //Monobehauvior
        if (groundCheck == null) return;                                                        //Monobehauvior
                                                                                                //Monobehauvior
        Gizmos.color = Color.red;                                                               //Monobehauvior
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);                              //Monobehauvior
    }
}
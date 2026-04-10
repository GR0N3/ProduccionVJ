using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    private InputSystem_Actions inputActions;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.right; // default

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Attack.performed += OnFire;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Attack.performed -= OnFire;

        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();

        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;
        }
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        Shoot();
    }

    void Shoot()
    {
        Vector2 shootDir = (movement != Vector2.zero)
            ? movement.normalized
            : lastDirection;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = shootDir * bulletSpeed;

        // Opcional: rotar bala
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
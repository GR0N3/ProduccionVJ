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

    public float spreadAngle = 30f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnFire;
        inputActions.Player.AltAttack.performed += OnAltFire;
    }                          
                               
    private void OnDisable()   
    {   
        inputActions.Player.Attack.performed -= OnFire;
        inputActions.Player.AltAttack.performed -= OnAltFire;
        inputActions.Disable();

    }

    void Update()
    {
        Vector2 dir = GetMouseDirection();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnAltFire(InputAction.CallbackContext ctx)
    {
        ShootSpread();
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
        ShootNormal();
    }

    void ShootNormal()
    {
        Vector2 dir = GetMouseDirection();
        FireBullet(dir);
    }

    void ShootSpread()
    {
        Vector2 baseDir = GetMouseDirection();

        FireBullet(baseDir);
        FireBullet(Rotate(baseDir, spreadAngle));
        FireBullet(Rotate(baseDir, -spreadAngle));
    }

    Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * direction;
    }

    void FireBullet(Vector2 dir)
    {

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        bullet.GetComponent<Bullet>().Init(dir);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * bulletSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    Vector2 GetMouseDirection()
    {
        Vector3 mouseScreen = Mouse.current.position.ReadValue();

        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        return (mouseWorld - firePoint.position).normalized;
    }

}
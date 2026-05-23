using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon
{
    private GameObject bulletPrefab;
    private Transform firePoint;
    private float bulletSpeed;
    private float bulletLifetime = 0.5f;
    private float bulletSpread;
    private int damage;
    private float knockbackforce;
    private int bulletsCount;

    private InputSystem_Actions inputActions;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.right; // default

    public void Init(PlayerController player)
    {                                                                   
        inputActions = player.InputActions;
        firePoint = player.FirePoint;
        damage = player.Damage;
        knockbackforce = player.KnockbackForce;
        bulletSpread = player.BulletSpread;
        bulletSpeed = player.BulletSpeed;
        bulletsCount = player.BulletsCount;
        bulletPrefab = player.BulletPrefab;
    }

    public void OnAltFire(InputAction.CallbackContext ctx)
    {
        ShootSpread();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();

        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;
        }
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        ShootNormal();
    }

    void ShootNormal()
    {
        Vector2 baseDir = (movement != Vector2.zero)
        ? movement.normalized
        : lastDirection;

        FireBullet(baseDir);
    }

    void ShootSpread()
    {
        Vector2 baseDir = (movement != Vector2.zero)
            ? movement.normalized
            : lastDirection;

        //for (int i =0; i<= bulletsCount; i++)
        //{
        //    FireBullet(baseDir);
        //} Cambiar x formula para los tiros blablabla

        FireBullet(baseDir); // centro
        FireBullet(Rotate(baseDir, bulletSpread));   // derecha
        FireBullet(Rotate(baseDir, -bulletSpread));  // izquierda
    }

    Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * direction;
    }

    void FireBullet(Vector2 dir)
    {

        GameObject bullet = ObjectPoolManager.SpawnObject(bulletPrefab, firePoint.position, Quaternion.identity);

        bullet.GetComponent<Bullet>().Init(dir, bulletLifetime, damage, knockbackforce);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * bulletSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
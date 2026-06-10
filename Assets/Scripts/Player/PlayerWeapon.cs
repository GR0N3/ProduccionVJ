using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon
{
    private GameObject bulletPrefab;
    private Transform firePoint;
    private float bulletSpeed;
    private float bulletLifetime;
    private float bulletSpread;
    private int damage;
    private float knockbackforce;
    private int bulletsCount;

    private InputSystem_Actions inputActions;
    private Vector2 lastDirection = Vector2.right;

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
        bulletLifetime = player.BulletLifetime;
    }

    public void OnAltFire(InputAction.CallbackContext ctx)
    {
        ShootSpread();
    }

    // AHORA LEE SOLO IZQUIERDA Y DERECHA
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        if (input.x > 0.1f) lastDirection = Vector2.right;
        else if (input.x < -0.1f) lastDirection = Vector2.left;
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        ShootNormal();
    }

    void ShootNormal()
    {
        FireBullet(lastDirection);
    }

    void ShootSpread()
    {
        FireBullet(lastDirection);
        FireBullet(Rotate(lastDirection, bulletSpread));
        FireBullet(Rotate(lastDirection, -bulletSpread));
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
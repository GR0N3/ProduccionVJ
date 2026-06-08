using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

public class PlayerWeapon
{
    private GameObject bulletPrefab;
    private Transform firePoint;
    private PlayerController playerController;
    private float bulletSpeed;
    private float bulletLifetime = 0.5f;
    private float bulletsSpread;
    private int damage;
    private float knockbackforce;
    private int bulletsCount;

    private InputSystem_Actions inputActions;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.right; // default

    public void Init(PlayerController player)
    {
        playerController = player;
        firePoint = player.FirePoint;
        damage = player.Damage;
        knockbackforce = player.KnockbackForce;
        bulletsSpread = player.BulletSpread;
        bulletSpeed = player.BulletSpeed;
        bulletsCount = player.BulletsCount;
        bulletPrefab = player.BulletPrefab;
    }

    public void SetMoveInput(Vector2 input)
    {
        movement = input;

        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;
        }
    }

    public void Fire()
    {
        ShootNormal();
        playerController.PlayAttackAnimation();
    }

    public void AltFire()
    {
        ShootSpread();
        playerController.PlayAttackAnimation();
    }

    public void OnAltFire(InputAction.CallbackContext ctx)
    {
        AltFire();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        SetMoveInput(ctx.ReadValue<Vector2>());
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        Fire();
    }

    private void ShootNormal()
    {
        Vector2 baseDir = (movement != Vector2.zero)
        ? movement.normalized
        : lastDirection;

        FireBullet(baseDir);
    }

    private void ShootSpread()
    {
        Vector2 baseDir = (movement != Vector2.zero)
            ? movement.normalized
            : lastDirection;

        //for (int i =0; i<= bulletsCount; i++)
        //{
        //    FireBullet(baseDir);
        //} Cambiar x formula para los tiros blablabla

        FireBullet(baseDir); // centro
        FireBullet(Rotate(baseDir, bulletsSpread));   // derecha
        FireBullet(Rotate(baseDir, -bulletsSpread));  // izquierda
    }

    private Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * direction;
    }

    private void FireBullet(Vector2 dir)
    {

        GameObject bullet = ObjectPoolManager.SpawnObject(bulletPrefab, firePoint.position, Quaternion.identity);

        bullet.GetComponent<Bullet>().Init(dir, bulletLifetime, damage, knockbackforce);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * bulletSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    #region Upgrades

    public void UpgradeDamage(int result) {damage = result;}
    public void UpgradeBulletSpeed(float result) {bulletSpeed = result;}
    public void UpgradeBulletsCount(int result) {bulletsCount = result;}
    public void UpgradeBulletsSpread(float result) {bulletsSpread = result;}
    public void UpgradeKnockbackForce(float result) {knockbackforce = result;}

    #endregion
}

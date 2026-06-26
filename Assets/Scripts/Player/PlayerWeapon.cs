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

    private AnimatorBrain anim = new();

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
        anim = player.AnimatorBrain;
    }

    public void SetMoveInput(Vector2 input)
    {
        movement = input;

        if (movement != Vector2.zero)
        {
            lastDirection = movement.normalized;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        SetMoveInput(ctx.ReadValue<Vector2>());
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        anim.Play(PlayerAnimations.Attack, lockAnimation: true);
    }

    public void Shoot()
    {
        Vector2 baseDir = (movement != Vector2.zero) ? movement.normalized : lastDirection;
        
        if (bulletsCount == 1)
        {
            FireBullet(baseDir);
            return;
        }

        float step = (bulletsSpread * 0.5f ) / (bulletsCount - 1);

        for (int i = 0; i < bulletsCount; i++) 
        {
            float angle = (i - (bulletsCount - 1)/ 2f) * bulletsSpread;
            FireBullet(Rotate(baseDir, angle));
        }

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

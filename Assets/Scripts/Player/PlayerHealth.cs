using System;
using UnityEngine;

public class PlayerHealth
{
    private int maxHealth;
    private int currentHealth;
    private bool isDead;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private AnimatorBrain anim = new();

    public void Init(PlayerController player)

    {
        anim = player.AnimatorBrain;
    }

    public void SetHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;

        isDead = false;
    }

    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        anim.Play(PlayerAnimations.Hurt, lockAnimation: false, overrideLock: true);
        

        OnPlayerDamaged?.Invoke();

        if (currentHealth <= 0) 
        {
            Death();
        }
    }

    public void GainHealth(int heal)
    {
        currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        OnPlayerHealed?.Invoke();
    }

    public void Death()
    {
        anim.Play(PlayerAnimations.Death, lockAnimation: true, overrideLock: true);
        OnPlayerDeath?.Invoke();

    }

    #region Upgrades
    public void UpgradeMaxHealth(int result) {maxHealth = result;}

    #endregion

}

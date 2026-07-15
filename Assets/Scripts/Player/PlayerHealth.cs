using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth
{
    private int maxHealth;
    private int currentHealth;
    private bool isDead;
    private PlayerController player;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    AnimatorBrain anim = new();

    public void Init(PlayerController player)
    {
        this.player = player;
        anim = player.AnimatorBrain;
        SetHealth(player.maxHealth);
    }

    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        isDead = false;
    }

    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (isDead || (player != null && player.IsGodMode))
        {
            return;
        }

        currentHealth -= damage;

        OnPlayerDamaged?.Invoke();
        if (currentHealth <= 0) 
        {
            anim.Play(PlayerAnimations.Death, overrideLock: true, lockAnimation: true);
            if (!anim.IsPlaying(PlayerAnimations.Death))
            {
                Death();
            }

        }
        else
        {
            anim.Play(PlayerAnimations.Hurt, overrideLock: true, lockAnimation:true);
        }
    }

    public void GainHealth(int heal)
    {
        currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        OnPlayerHealed?.Invoke();
    }

    public void Death()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnPlayerDeath?.Invoke();
    }

    #region Upgrades
    public void UpgradeMaxHealth(int result) 
    {
        SetHealth(result);
    }

    #endregion

}

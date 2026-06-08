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

    public void Init(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        Debug.Log("took damage: " + currentHealth);

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
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnPlayerDeath?.Invoke();
    }

    #region Upgrades
    public void UpgradeMaxHealth(int result) {maxHealth = result;}

    #endregion

}

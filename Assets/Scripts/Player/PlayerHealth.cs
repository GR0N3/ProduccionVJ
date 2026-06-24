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

    private AnimationController anim = new();


    public void Init(PlayerController player)
    {
        anim.Init(player.Animator);
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

        anim.Play(PlayerAnimations.Hurt);

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
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDataBase.Scenes.Match)
            .Unload(SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perfrom();
        OnPlayerDeath?.Invoke();
    }

    #region Upgrades
    public void UpgradeMaxHealth(int result) {maxHealth = result;}

    #endregion

}

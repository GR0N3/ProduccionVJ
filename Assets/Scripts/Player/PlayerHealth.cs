using System;
using UnityEngine;

public class PlayerHealth
{
    private int maxHealth = 10;
    private int currentHealth;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public void Init(int MaxHealth)
    {
        currentHealth = MaxHealth;
    }

    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
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
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDataBase.Scenes.Match)
            .Unload(SceneDataBase.Scenes.Session)
            .Load(SceneDataBase.Slots.Menu, SceneDataBase.Scenes.MainMenu)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perfrom(); 
    }
}
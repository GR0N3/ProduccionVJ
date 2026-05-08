using System;
using UnityEngine;

public class PlayerHealth 
{
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public PlayerHealth()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnPlayerDamaged!.Invoke();
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

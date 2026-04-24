using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int currentHealth;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnPlayerDamaged!.Invoke();
    }

    public void GainHealth(int heal)
    {
        if((currentHealth += heal) < maxHealth)
            currentHealth += heal;
        OnPlayerHealed!.Invoke();
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

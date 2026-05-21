using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerHealed;
    public static event Action OnPlayerDeath;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        SessionController.Instance.PlayerManager.playerHealth = this;
    }
    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce)
    {
        currentHealth -= damage;
        Debug.Log("took damage: " + currentHealth);
        //OnPlayerDamaged!.Invoke();
        ApplyKnockback(hitDirection, knockbackForce);

        if (currentHealth <= 0) 
        {
            Death();
        }
    }

    void ApplyKnockback(Vector2 direction, float force)
    {
        Vector2 finalForce = direction.normalized * force;

        rb.AddForce(finalForce, ForceMode2D.Impulse);
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

    private void OnCollisionEnter2D(Collision2D collision)
    { 
        if ((hitMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            TakeDamage(1,new Vector2(-1,-1), 25f);
        }

    }

}

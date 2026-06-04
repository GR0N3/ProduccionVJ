using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public GameObject heartsPrefab;
    private List<HealthHeart> hearts = new List<HealthHeart>();
    private PlayerManager playerManager;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDamaged += Drawhearts;
        PlayerHealth.OnPlayerHealed += Drawhearts;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDamaged -= Drawhearts;
        PlayerHealth.OnPlayerHealed -= Drawhearts;
    }
    void Start()
    {
        playerManager = ServiceLocator.Get<PlayerManager>();

        Drawhearts();
    }
    public void Drawhearts()
    {
        Clearhearts();

        //cuantos corazones hay en total

        float maxHealthRemainder = playerManager.PlayerHealth.CurrentHealth % 2;
        int heartsToMake = (int)((playerManager.PlayerHealth.MaxHealth / 2) + maxHealthRemainder);
        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyhearts();
        }
        for (int i = 0; i < hearts.Count; i++)
        {
            int heartsStatusRemainder = Mathf.Clamp(playerManager.PlayerHealth.CurrentHealth - (i * 2), 0, 2);
            hearts[i].SetHeartImage((HeartStatus)heartsStatusRemainder);
        }

    }

    public void CreateEmptyhearts()
    {
        GameObject newhearts = Instantiate(heartsPrefab);
        newhearts.transform.SetParent(transform, false);

        HealthHeart heartsComponent = newhearts.GetComponent<HealthHeart>();
        heartsComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartsComponent);
    }

    public void Clearhearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<HealthHeart>();
    }
}

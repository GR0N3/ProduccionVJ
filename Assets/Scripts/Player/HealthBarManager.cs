using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public GameObject heartsPrefab;
    private PlayerHealth playerHealth;
    List<HealthHeart> hearts = new List<HealthHeart>();

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
        playerHealth = SessionController.Instance.PlayerManager.PlayerHealth;
        Drawhearts();
    }
    public void Drawhearts()
    {
        Clearhearts();

        //cuantos corazones hay en total

        float maxHealthRemainder = SessionController.Instance.PlayerManager.PlayerHealth.CurrentHealth % 2;
        int heartsToMake = (int)((SessionController.Instance.PlayerManager.PlayerHealth.MaxHealth / 2) + maxHealthRemainder);
        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyhearts();
        }
        for (int i = 0; i < hearts.Count; i++)
        {
            int heartsStatusRemainder = Mathf.Clamp(playerHealth.CurrentHealth - (i * 2), 0, 2);
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

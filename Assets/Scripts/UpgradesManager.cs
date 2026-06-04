using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UpgradesManager : MonoBehaviour
{
    private PlayerManager playerManager;

    [SerializeField] private int upgradesCant;
    [SerializeField] private List<GameObject> upgradePrefab;
    [SerializeField] private GameObject upgradesParent;
    private void Awake()
    {
        ServiceLocator.Register<UpgradesManager>(this);

        playerManager = ServiceLocator.Get<PlayerManager>();
        
    }
    private void SpawnUpgrades() 
    {
        for (int i = 0; i < upgradesCant; i++) 
        {
            int random = Random.Range(0, upgradesCant);
            Instantiate(upgradePrefab[random], upgradesParent.transform);
        }
    }
    public void Upgrade(ShopUpgrade upgrade)
    {
        foreach (var modifier in upgrade.modifiers)
        {
            if (modifier.multiplicative)
            {
                playerManager.Stats.MultiplyStat(modifier.stat, modifier.value);
            }
            else
            {
                playerManager.Stats.AddStat(modifier.stat,modifier.value);
            }
        }
    }
    private void OnDestroy()
    {
        ServiceLocator.Unregister<UpgradesManager>();
    }
}

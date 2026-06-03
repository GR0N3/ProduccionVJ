using UnityEngine;
using UnityEngine.Rendering;

public class UpgradesManager : MonoBehaviour
{
    PlayerManager playerManager;

    private void Awake()
    {
        ServiceLocator.Register<UpgradesManager>(this);

        playerManager = ServiceLocator.Get<PlayerManager>();
        
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

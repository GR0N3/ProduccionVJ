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

    private void OnDestroy()
    {
        ServiceLocator.Unregister<UpgradesManager>();
    }

    public void UpgradeMovementSpeed(ShopUpgrade upgradeData)
    {
        playerManager.PlayerMovement.UpgradeSpeed(upgradeData.multiplier);
    }
}

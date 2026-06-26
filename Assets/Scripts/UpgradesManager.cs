using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    private SessionController controller;
    private PlayerManager playerManager;

    [SerializeField] private int upgradesCant;
    [SerializeField] private GameObject upgradeCardPrefab;
    [SerializeField] private List<ShopUpgrade> allUpgrades;
    [SerializeField] private GameObject upgradesParent;

    private List<GameObject> upgradesList = new();

    private void Awake()
    {
        ServiceLocator.Register<UpgradesManager>(this);

        controller = ServiceLocator.Get<SessionController>();

        if (ServiceLocator.TryGet(out PlayerManager registeredPlayerManager))
        {
            playerManager = registeredPlayerManager;
        }
        else
        {
            playerManager = FindAnyObjectByType<PlayerManager>();

            if (playerManager != null)
            {
                ServiceLocator.Register(playerManager);
            }
        }
        
    }

    private void Start()
    {
        SpawnUpgrades();
    }

    private void SpawnUpgrades()
    {
        ClearUpgrades();

        for (int i = 0; i < upgradesCant; i++)
        {
            int random = Random.Range(0, allUpgrades.Count);

            GameObject card = Instantiate(upgradeCardPrefab);
            card.transform.SetParent(upgradesParent.transform, false);

            card.GetComponent<Upgrade>().Init(allUpgrades[random]);

            upgradesList.Add(card);
        }
    }

    private void ClearUpgrades()
    {
        foreach (Transform t in upgradesParent.transform)
        {
            Destroy(t.gameObject);
        }
        upgradesList.Clear();
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


        controller.SubtractPoints(upgrade.cost);

    }

    public void RerollUpgrades()
    {
        SpawnUpgrades();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<UpgradesManager>();
    }
}

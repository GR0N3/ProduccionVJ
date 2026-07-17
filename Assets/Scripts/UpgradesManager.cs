using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesManager : MonoBehaviour
{
    private SessionController controller;
    private PlayerManager playerManager;
    private HealthBarManager healthBarManager;
    [SerializeField] private int upgradesCant;
    [SerializeField] private GameObject upgradeCardPrefab;
    [SerializeField] private List<ShopUpgrade> allUpgrades;
    [SerializeField] private GameObject upgradesParent;

    [Header("Probabilidad por Tier y Nivel (%) - Index 0 = Nvl1")]
    [SerializeField] private float[] tierSWeights = { 0.7f, 2f, 4f, 6.7f, 10f };
    [SerializeField] private float[] tierAWeights = { 4f, 7f, 10f, 12.5f, 12.5f };
    [SerializeField] private float[] tierBWeights = { 10f, 11.7f, 11.7f, 11f, 8.3f };
    [SerializeField] private float[] tierCWeights = { 12f, 9f, 6.6f, 5f, 4f };

    [Header("Aparición secuencial")]
    [SerializeField] private float delayBetweenCards = 1.7f;

    [Header("Límites de la tienda")]
    [SerializeField] private Button rerollButton;

    private bool hasRerolled = false;
    private bool hasPurchased = false;

    private List<GameObject> upgradesList = new();
    private Coroutine spawnRoutine;

    private void Awake()
    {
        ServiceLocator.Register<UpgradesManager>(this);
        controller = ServiceLocator.Get<SessionController>();
        playerManager = ServiceLocator.Get<PlayerManager>();
        healthBarManager = ServiceLocator.Get<HealthBarManager>();
    }

    private void Start()
    {
        hasRerolled = false;
        hasPurchased = false;
        UpdateRerollButtonState();
        SpawnUpgrades();
    }

    private float GetWeightForTier(Tier tier, int levelIndex)
    {
        float[] weights = tier switch
        {
            Tier.C => tierCWeights,
            Tier.B => tierBWeights,
            Tier.A => tierAWeights,
            Tier.S => tierSWeights,
            _ => tierCWeights
        };
        int index = Mathf.Clamp(levelIndex, 0, weights.Length - 1);
        return weights[index];
    }

    private ShopUpgrade GetRandomUpgradeWeighted(int levelIndex)
    {
        float totalWeight = 0f;
        foreach (var upgrade in allUpgrades)
        {
            totalWeight += GetWeightForTier(upgrade.tier, levelIndex);
        }
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var upgrade in allUpgrades)
        {
            cumulative += GetWeightForTier(upgrade.tier, levelIndex);
            if (roll <= cumulative)
                return upgrade;
        }
        return allUpgrades[allUpgrades.Count - 1];
    }

    private void SpawnUpgrades()
    {
        ClearUpgrades();
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnUpgradesRoutine());
    }

    private IEnumerator SpawnUpgradesRoutine()
    {
        int levelIndex = controller.SceneIndex;
        for (int i = 0; i < upgradesCant; i++)
        {
            ShopUpgrade selected = GetRandomUpgradeWeighted(levelIndex);
            GameObject card = Instantiate(upgradeCardPrefab);
            card.transform.SetParent(upgradesParent.transform, false);
            card.GetComponent<Upgrade>().Init(selected);
            upgradesList.Add(card);
            yield return new WaitForSeconds(delayBetweenCards);
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
        if (hasPurchased) return;

        foreach (var modifier in upgrade.modifiers)
        {
            if (modifier.multiplicative)
            {
                playerManager.Stats.MultiplyStat(modifier.stat, modifier.value);
            }
            else
            {
                playerManager.Stats.AddStat(modifier.stat, modifier.value);
            }
        }
        controller.SubtractPoints(upgrade.cost);
        playerManager.PlayerHealth.SetHealth(playerManager.PlayerHealth.MaxHealth);
        healthBarManager.DrawHearts();

        hasPurchased = true;
        UpdateRerollButtonState();
    }

    public void RerollUpgrades()
    {
        if (hasRerolled || hasPurchased) return;

        hasRerolled = true;
        UpdateRerollButtonState();
        SpawnUpgrades();
    }

    private void UpdateRerollButtonState()
    {
        if (rerollButton == null) return;
        rerollButton.interactable = !hasRerolled && !hasPurchased;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<UpgradesManager>();
    }
}
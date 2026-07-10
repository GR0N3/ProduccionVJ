using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    private ShopUpgrade upgrade;
    private UpgradesManager upgradeManager;
    private SessionController sessionController;
    private string description;
    private string price;

    [SerializeField] private Image image;
    [SerializeField] private Image colour;
    [SerializeField] private TMP_Text Description_Text;
    [SerializeField] private TMP_Text Price_Text;

    [Header("Fade por Tier")]
    [SerializeField] private CanvasGroup canvasGroup;

    // Index 0 = C, 1 = B, 2 = A, 3 = S (mismo orden que el enum Tier)
    [SerializeField] private float[] fadeDurationByTier = { 0.3f, 0.6f, 1f, 1.5f };

    private Coroutine fadeRoutine;

    private void Start()
    {
        upgradeManager = ServiceLocator.Get<UpgradesManager>();
        sessionController = ServiceLocator.Get<SessionController>();
    }

    public void Init(ShopUpgrade data)
    {
        upgrade = data;
        image.sprite = upgrade.Image;
        colour.color = upgrade.colour;
        description = upgrade.description;
        price = upgrade.cost + "\n Points";
        Description_Text.text = description;
        Price_Text.text = price;

        StartFadeIn();
    }

    private void StartFadeIn()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        float duration = GetFadeDurationForTier(upgrade.tier);
        fadeRoutine = StartCoroutine(FadeInRoutine(duration));
    }

    private float GetFadeDurationForTier(Tier tier)
    {
        int index = Mathf.Clamp((int)tier, 0, fadeDurationByTier.Length - 1);
        return fadeDurationByTier[index];
    }

    private IEnumerator FadeInRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public void SelectUpgrade()
    {
        if (upgrade.cost <= (int)sessionController.Points)
            upgradeManager.Upgrade(upgrade);
    }
}
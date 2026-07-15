using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 0.55f;
    [SerializeField] private float fadeOutTime = 0.45f;
    [SerializeField] private Color backgroundColor = new(0.03f, 0.04f, 0.09f, 1f);
    [SerializeField] private Color primaryAccent = new(0.15f, 0.96f, 0.98f, 0.92f);
    [SerializeField] private Color secondaryAccent = new(0.58f, 0.22f, 1f, 0.88f);
    [SerializeField] private Color centerGlowColor = new(1f, 1f, 1f, 0.95f);

    private RectTransform overlayRect;
    private Image backgroundImage;
    private RectTransform leftShutter;
    private RectTransform centerShutter;
    private RectTransform rightShutter;
    private Image leftShutterImage;
    private Image centerShutterImage;
    private Image rightShutterImage;
    private RectTransform centerDiamond;
    private Image centerDiamondImage;
    private RectTransform centerCore;
    private Image centerCoreImage;

    public IEnumerator FadeInBlack()
    {
        EnsureVisuals();
        UpdateVisualLayout();
        yield return AnimateCover();
    }

    public IEnumerator FadeOutBlack()
    {
        EnsureVisuals();
        UpdateVisualLayout();
        yield return AnimateReveal();
    }

    private void Awake()
    {
        EnsureVisuals();
        SetHiddenState();
    }

    private void EnsureVisuals()
    {
        EnsureCanvasHierarchyReady();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (!canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        overlayRect = canvasGroup.transform as RectTransform;
        if (overlayRect == null)
        {
            return;
        }

        backgroundImage = EnsureImage("Background", backgroundColor, out _);
        leftShutterImage = EnsureImage("LeftShutter", primaryAccent, out leftShutter);
        centerShutterImage = EnsureImage("CenterShutter", secondaryAccent, out centerShutter);
        rightShutterImage = EnsureImage("RightShutter", primaryAccent, out rightShutter);
        centerDiamondImage = EnsureImage("CenterDiamond", secondaryAccent, out centerDiamond);
        centerCoreImage = EnsureImage("CenterCore", centerGlowColor, out centerCore);

        canvasGroup.interactable = false;
    }

    private void EnsureCanvasHierarchyReady()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        if (parentCanvas != null)
        {
            if (!parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
            }

            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            if (canvasRect != null && canvasRect.localScale == Vector3.zero)
            {
                canvasRect.localScale = Vector3.one;
            }

            parentCanvas.sortingOrder = 32767;
        }
    }

    private Image EnsureImage(string objectName, Color color, out RectTransform rectTransform)
    {
        Transform child = overlayRect.Find(objectName);
        if (child == null)
        {
            GameObject childObject = new(objectName, typeof(RectTransform), typeof(Image));
            childObject.transform.SetParent(overlayRect, false);
            child = childObject.transform;
        }

        rectTransform = child as RectTransform;
        Image image = child.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void UpdateVisualLayout()
    {
        if (overlayRect == null)
        {
            return;
        }

        float width = overlayRect.rect.width > 0f ? overlayRect.rect.width : Screen.width;
        float height = overlayRect.rect.height > 0f ? overlayRect.rect.height : Screen.height;
        float shutterWidth = width * 0.75f;
        float shutterHeight = height * 2.1f;
        float diamondSize = Mathf.Min(width, height) * 0.18f;

        StretchFullScreen(backgroundImage.rectTransform);

        ConfigureShutter(leftShutter, shutterWidth, shutterHeight, -18f);
        ConfigureShutter(centerShutter, shutterWidth * 0.9f, shutterHeight, 8f);
        ConfigureShutter(rightShutter, shutterWidth, shutterHeight, 18f);

        centerDiamond.anchorMin = new Vector2(0.5f, 0.5f);
        centerDiamond.anchorMax = new Vector2(0.5f, 0.5f);
        centerDiamond.pivot = new Vector2(0.5f, 0.5f);
        centerDiamond.sizeDelta = Vector2.one * diamondSize;
        centerDiamond.localRotation = Quaternion.Euler(0f, 0f, 45f);

        centerCore.anchorMin = new Vector2(0.5f, 0.5f);
        centerCore.anchorMax = new Vector2(0.5f, 0.5f);
        centerCore.pivot = new Vector2(0.5f, 0.5f);
        centerCore.sizeDelta = Vector2.one * (diamondSize * 0.42f);
        centerCore.localRotation = Quaternion.identity;
    }

    private void StretchFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private void ConfigureShutter(RectTransform rectTransform, float width, float height, float angle)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        rectTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateCover()
    {
        SetHiddenState();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        Vector2 hiddenLeft = GetHiddenPosition(-1.35f);
        Vector2 hiddenCenter = GetHiddenPosition(-1.55f);
        Vector2 hiddenRight = GetHiddenPosition(-1.75f);
        Vector2 shownLeft = GetShownPosition(-0.28f);
        Vector2 shownCenter = GetShownPosition(0f);
        Vector2 shownRight = GetShownPosition(0.28f);

        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInTime);

            SetImageAlpha(backgroundImage, Smooth01(progress * 1.15f));
            AnimateShutter(leftShutter, leftShutterImage, hiddenLeft, shownLeft, progress, 0f, 0.62f, 0.82f);
            AnimateShutter(centerShutter, centerShutterImage, hiddenCenter, shownCenter, progress, 0.07f, 0.58f, 0.88f);
            AnimateShutter(rightShutter, rightShutterImage, hiddenRight, shownRight, progress, 0.14f, 0.56f, 0.82f);

            float diamondProgress = Mathf.Clamp01((progress - 0.08f) / 0.55f);
            float glowFade = 1f - Mathf.Clamp01((progress - 0.72f) / 0.28f);
            centerDiamond.anchoredPosition = Vector2.zero;
            centerDiamond.localScale = Vector3.one * Mathf.LerpUnclamped(0.2f, 1.18f, EaseOutBack(diamondProgress));
            centerDiamond.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(45f, 160f, Smooth01(progress)));
            SetImageAlpha(centerDiamondImage, 0.55f * glowFade);

            centerCore.anchoredPosition = Vector2.zero;
            centerCore.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.35f, Smooth01(diamondProgress));
            SetImageAlpha(centerCoreImage, 0.9f * glowFade);

            yield return null;
        }

        SetCoveredState(shownLeft, shownCenter, shownRight);
    }

    private IEnumerator AnimateReveal()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Vector2 shownLeft = GetShownPosition(-0.28f);
        Vector2 shownCenter = GetShownPosition(0f);
        Vector2 shownRight = GetShownPosition(0.28f);
        Vector2 exitLeft = GetHiddenPosition(1.45f);
        Vector2 exitCenter = GetHiddenPosition(1.65f);
        Vector2 exitRight = GetHiddenPosition(1.85f);

        SetCoveredState(shownLeft, shownCenter, shownRight);

        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeOutTime);

            float backgroundFadeStart = 0.32f;
            float backgroundProgress = Mathf.Clamp01((progress - backgroundFadeStart) / (1f - backgroundFadeStart));
            SetImageAlpha(backgroundImage, 1f - Smooth01(backgroundProgress));

            AnimateShutter(leftShutter, leftShutterImage, shownLeft, exitLeft, progress, 0f, 0.55f, 0.82f);
            AnimateShutter(centerShutter, centerShutterImage, shownCenter, exitCenter, progress, 0.06f, 0.55f, 0.88f);
            AnimateShutter(rightShutter, rightShutterImage, shownRight, exitRight, progress, 0.12f, 0.55f, 0.82f);

            centerDiamond.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, EaseInCubic(progress));
            centerDiamond.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(160f, 260f, Smooth01(progress)));
            SetImageAlpha(centerDiamondImage, 0.32f * (1f - Smooth01(progress)));

            centerCore.localScale = Vector3.one * Mathf.Lerp(1f, 0.1f, EaseInCubic(progress));
            SetImageAlpha(centerCoreImage, 0.75f * (1f - Smooth01(progress)));

            yield return null;
        }

        SetHiddenState();
    }

    private void AnimateShutter(
        RectTransform shutter,
        Image shutterImage,
        Vector2 from,
        Vector2 to,
        float progress,
        float delay,
        float duration,
        float maxAlpha)
    {
        float localProgress = Mathf.Clamp01((progress - delay) / duration);
        shutter.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutCubic(localProgress));
        SetImageAlpha(shutterImage, Mathf.Lerp(0f, maxAlpha, Smooth01(localProgress)));
    }

    private Vector2 GetHiddenPosition(float horizontalFactor)
    {
        float width = overlayRect.rect.width > 0f ? overlayRect.rect.width : Screen.width;
        return new Vector2(width * horizontalFactor, 0f);
    }

    private Vector2 GetShownPosition(float horizontalFactor)
    {
        float width = overlayRect.rect.width > 0f ? overlayRect.rect.width : Screen.width;
        return new Vector2(width * horizontalFactor, 0f);
    }

    private void SetCoveredState(Vector2 shownLeft, Vector2 shownCenter, Vector2 shownRight)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        SetImageAlpha(backgroundImage, 1f);

        leftShutter.anchoredPosition = shownLeft;
        centerShutter.anchoredPosition = shownCenter;
        rightShutter.anchoredPosition = shownRight;
        SetImageAlpha(leftShutterImage, 0.82f);
        SetImageAlpha(centerShutterImage, 0.88f);
        SetImageAlpha(rightShutterImage, 0.82f);

        centerDiamond.anchoredPosition = Vector2.zero;
        centerDiamond.localScale = Vector3.one;
        centerDiamond.localRotation = Quaternion.Euler(0f, 0f, 160f);
        SetImageAlpha(centerDiamondImage, 0.18f);

        centerCore.anchoredPosition = Vector2.zero;
        centerCore.localScale = Vector3.one * 0.85f;
        SetImageAlpha(centerCoreImage, 0.25f);
    }

    private void SetHiddenState()
    {
        if (overlayRect == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        SetImageAlpha(backgroundImage, 0f);
        SetImageAlpha(leftShutterImage, 0f);
        SetImageAlpha(centerShutterImage, 0f);
        SetImageAlpha(rightShutterImage, 0f);
        SetImageAlpha(centerDiamondImage, 0f);
        SetImageAlpha(centerCoreImage, 0f);

        leftShutter.anchoredPosition = GetHiddenPosition(-1.35f);
        centerShutter.anchoredPosition = GetHiddenPosition(-1.55f);
        rightShutter.anchoredPosition = GetHiddenPosition(-1.75f);
        centerDiamond.anchoredPosition = Vector2.zero;
        centerDiamond.localScale = Vector3.one * 0.2f;
        centerDiamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
        centerCore.anchoredPosition = Vector2.zero;
        centerCore.localScale = Vector3.one * 0.1f;

        if (canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }

    private void SetImageAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
        {
            return;
        }

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - (2f * value));
    }

    private float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        float inverse = 1f - value;
        return 1f - (inverse * inverse * inverse);
    }

    private float EaseInCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * value;
    }

    private float EaseOutBack(float value)
    {
        value = Mathf.Clamp01(value);
        const float overshoot = 1.70158f;
        float adjusted = value - 1f;
        return 1f + ((overshoot + 1f) * adjusted * adjusted * adjusted) + (overshoot * adjusted * adjusted);
    }
}

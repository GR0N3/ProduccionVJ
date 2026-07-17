using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private GameObject heartsPrefab;

    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int maxColumns = 10;
    [SerializeField] private Vector2 spacing = new Vector2(4, 4);

    private readonly List<HealthHeart> hearts = new();

    private PlayerManager playerManager;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDamaged += DrawHearts;
        PlayerHealth.OnPlayerHealed += DrawHearts;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDamaged -= DrawHearts;
        PlayerHealth.OnPlayerHealed -= DrawHearts;
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        ServiceLocator.Register(this);
        playerManager = ServiceLocator.Get<PlayerManager>();
        DrawHearts();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
            return;

        UpdateGrid();
    }

    public void DrawHearts()
    {
        ClearHearts();

        for (int i = 0; i < playerManager.PlayerHealth.CurrentHealth; i++)
        {
            CreateHeart();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            GetComponent<RectTransform>());
    }

    private void CreateHeart()
    {
        GameObject newHeart = Instantiate(heartsPrefab, transform, false);
        var heartClass = newHeart.GetComponent<HealthHeart>();
        heartClass.SetHeartImage(HeartStatus.Full);
        hearts.Add(heartClass);
    }

    private void ClearHearts()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        hearts.Clear();
    }

    private void UpdateGrid()
    {
        RectTransform rect = (RectTransform)transform;

        gridLayout.spacing = spacing;

        int heartCount = Mathf.Max(playerManager.PlayerHealth.CurrentHealth, 1);

        // Cantidad de columnas (hasta maxColumns)
        int columns = Mathf.Min(heartCount, maxColumns);

        // Cantidad de filas necesarias
        int rows = Mathf.CeilToInt((float)heartCount / columns);

        float availableWidth = rect.rect.width - spacing.x * (columns - 1);
        float availableHeight = rect.rect.height - spacing.y * (rows - 1);

        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        // Corazones cuadrados
        float size = Mathf.Min(cellWidth, cellHeight);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.cellSize = new Vector2(size, size);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<HealthBarManager>();
    }
}
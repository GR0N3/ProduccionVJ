using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Upgrade : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    private ShopUpgrade upgrade;

    private UpgradesManager upgradeManager;

    private SessionController sessionController;

    private string description;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text Ui_Text;
    [SerializeField] private GameObject togglePanel;
    private void Awake()
    {
        upgradeManager = ServiceLocator.Get<UpgradesManager>();
        sessionController = ServiceLocator.Get<SessionController>();
    }

    private void Start()
    {
        togglePanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        togglePanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        togglePanel.SetActive(false);
    }
    public void Init(ShopUpgrade data)
    {
        upgrade = data;

        image.sprite = upgrade.Image;
        description = upgrade.description + "\n Cost:" + upgrade.cost;

        Ui_Text.text = description;
    }

    public void SelectUpgrade()
    {
        if (upgrade.cost < sessionController.Gold)
            upgradeManager.Upgrade(upgrade);
    }
}

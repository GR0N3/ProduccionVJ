using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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


    }

    public void SelectUpgrade()
    {
        if (upgrade.cost <= (int)sessionController.Points)
            upgradeManager.Upgrade(upgrade);
    }
}

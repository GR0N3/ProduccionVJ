using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Upgrade : MonoBehaviour
{
    private Image image;
    private string description;


    public void Init(ShopUpgrade upgradeData)
    {
        image = GetComponent<Image>();
        image = upgradeData.Image;

        var text = GetComponent<TMP_Text>();
        description = upgradeData.description;
        text.text = description;

    }
}

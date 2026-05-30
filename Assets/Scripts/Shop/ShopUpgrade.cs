using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(menuName = "Upgrade/")]
public class ShopUpgrade : ScriptableObject
{
    public UpgradeType type;
    public Image Image;
    public string description;
    public float multiplier;
    public int level;

}

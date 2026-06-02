using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Shop Upgrade", order = 1)]
public class ShopUpgrade : ScriptableObject
{
    public UpgradeType type;
    public Image Image;
    public string description;
    public float multiplier;
    public int level;
}

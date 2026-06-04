using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Upgrade/Stat Upgrade")]
public class ShopUpgrade : ScriptableObject
{
    public List<StatsModifier> modifiers;
    public Image Image;
    public string description;
    public bool isMultiplier;
    public int level;
}

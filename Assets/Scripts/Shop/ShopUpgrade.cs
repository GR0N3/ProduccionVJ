using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Upgrade/Stat Upgrade")]
public class ShopUpgrade : ScriptableObject
{
    public List<StatsModifier> modifiers;
    public Sprite Image;
    public string description;
    public int level;
    public int cost;
}

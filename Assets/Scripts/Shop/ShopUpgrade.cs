using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Upgrade/Stat Upgrade")]

public abstract class ShopUpgrade : ScriptableObject
{
    public List<StatsModifier> modifiers;
    public Image Image;
    public string description;
    public float value;
    public bool isMultiplier;
    public int level;
}

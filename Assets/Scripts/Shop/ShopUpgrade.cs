using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Tier
{
    C,
    B,
    A,
    S
}

[CreateAssetMenu(menuName = "Upgrade/Stat Upgrade")]
public class ShopUpgrade : ScriptableObject
{
    public List<StatsModifier> modifiers;
    public Sprite Image;
    public Color colour;
    public string description;
    public Tier tier;
    public int cost;
}
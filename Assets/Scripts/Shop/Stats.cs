using System.Collections.Generic;

public class Stats
{
    private Dictionary<UpgradeType, float> stats = new();

    public void SetStat(UpgradeType stat, float value)
    {
        stats[stat] = value;
    }

    public float GetStat(UpgradeType stat)
    {
        return stats.GetValueOrDefault(stat);
    }

    public void AddStat(UpgradeType stat, float value)
    {
        stats[stat] += value;
    }

    public void MultiplyStat(UpgradeType stat, float multiplier)
    {
        stats[stat] *= multiplier;
    }
}
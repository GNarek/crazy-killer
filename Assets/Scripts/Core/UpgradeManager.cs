using UnityEngine;

public static class UpgradeManager
{
    public enum UpgradeType { Damage, FireRate, WallHealth }

    private const int MaxLevel = 10;
    private const int BaseCost = 20;
    private const float CostGrowth = 1.35f;

    public static int GetLevel(UpgradeType type)
    {
        return PlayerPrefs.GetInt(Key(type), 0);
    }

    public static bool IsMaxLevel(UpgradeType type)
    {
        return GetLevel(type) >= MaxLevel;
    }

    public static int GetCost(UpgradeType type)
    {
        return Mathf.RoundToInt(BaseCost * Mathf.Pow(CostGrowth, GetLevel(type)));
    }

    public static bool TryPurchase(UpgradeType type)
    {
        if (IsMaxLevel(type)) return false;
        if (!CurrencyManager.SpendCoins(GetCost(type))) return false;

        PlayerPrefs.SetInt(Key(type), GetLevel(type) + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static float GetDamageBonus() => GetLevel(UpgradeType.Damage) * 0.5f;
    public static float GetFireRateBonus() => GetLevel(UpgradeType.FireRate) * 0.15f;
    public static float GetWallHealthBonus() => GetLevel(UpgradeType.WallHealth) * 5f;

    private static string Key(UpgradeType type) => $"Upgrade_{type}";
}

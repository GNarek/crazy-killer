using UnityEngine;

public static class ShooterManager
{
    public enum ShooterType { Standard, Rapid, Heavy }

    public const int MaxTier = 3;

    private const string SelectedKey = "SelectedShooter";

    public static ShooterType Selected
    {
        get => (ShooterType)PlayerPrefs.GetInt(SelectedKey, (int)ShooterType.Standard);
        set
        {
            PlayerPrefs.SetInt(SelectedKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsUnlocked(ShooterType type)
    {
        return type == ShooterType.Standard || PlayerPrefs.GetInt(UnlockKey(type), 0) == 1;
    }

    public static bool TryUnlock(ShooterType type)
    {
        if (IsUnlocked(type)) return true;
        if (!CurrencyManager.SpendCoins(GetCost(type))) return false;

        PlayerPrefs.SetInt(UnlockKey(type), 1);
        PlayerPrefs.Save();
        return true;
    }

    public static int GetCost(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => 150,
            ShooterType.Heavy => 300,
            _ => 0
        };
    }

    public static string GetName(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => "RAPID",
            ShooterType.Heavy => "HEAVY",
            _ => "STANDARD"
        };
    }

    public static float GetDamage(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => 1.2f,
            ShooterType.Heavy => 4.5f,
            _ => 2f
        };
    }

    public static float GetFireRate(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => 2.8f,
            ShooterType.Heavy => 0.8f,
            _ => 1.5f
        };
    }

    public static float GetProjectileSpeed(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => 14f,
            ShooterType.Heavy => 10f,
            _ => 12f
        };
    }

    public static Color GetColor(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => new Color(0.75f, 1f, 0.1f),
            ShooterType.Heavy => new Color(1f, 0.15f, 0.05f),
            _ => new Color(0.2f, 0.5f, 1f)
        };
    }

    public static Vector3 GetScale(ShooterType type)
    {
        return type switch
        {
            ShooterType.Rapid => new Vector3(0.6f, 0.6f, 0.6f),
            ShooterType.Heavy => new Vector3(1.7f, 1.7f, 1.7f),
            _ => Vector3.one
        };
    }

    public static float GetTierDamageMultiplier(int tier)
    {
        return tier switch
        {
            2 => 2f,
            3 => 4f,
            _ => 1f
        };
    }

    public static float GetTierScaleMultiplier(int tier)
    {
        return tier switch
        {
            2 => 1.3f,
            3 => 1.6f,
            _ => 1f
        };
    }

    private static string UnlockKey(ShooterType type) => $"ShooterUnlocked_{type}";
}

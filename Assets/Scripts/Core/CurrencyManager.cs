using UnityEngine;

public static class CurrencyManager
{
    private const string CoinsKey = "Coins";

    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    public static void AddCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
    }

    public static bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        PlayerPrefs.SetInt(CoinsKey, Coins - amount);
        PlayerPrefs.Save();
        return true;
    }
}

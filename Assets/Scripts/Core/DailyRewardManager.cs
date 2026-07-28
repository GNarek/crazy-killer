using System;
using System.Globalization;
using UnityEngine;

public static class DailyRewardManager
{
    private const string LastClaimKey = "DailyReward_LastClaimDate";
    private const string StreakKey = "DailyReward_Streak";
    private const string DateFormat = "yyyy-MM-dd";

    private static readonly int[] Rewards = { 20, 30, 40, 60, 80, 100, 200 };

    public static int DayCount => Rewards.Length;

    public static bool CanClaimToday()
    {
        return PlayerPrefs.GetString(LastClaimKey, "") != Today();
    }

    public static int NextRewardDay()
    {
        string last = PlayerPrefs.GetString(LastClaimKey, "");
        if (string.IsNullOrEmpty(last)) return 1;

        DateTime lastDate = DateTime.ParseExact(last, DateFormat, CultureInfo.InvariantCulture);
        int streak = PlayerPrefs.GetInt(StreakKey, 0);

        bool continuingStreak = lastDate == DateTime.Now.Date.AddDays(-1);
        if (!continuingStreak) return 1;

        return streak >= Rewards.Length ? 1 : streak + 1;
    }

    public static int GetRewardForDay(int day)
    {
        return Rewards[Mathf.Clamp(day - 1, 0, Rewards.Length - 1)];
    }

    public static bool TryClaim(out int coinsAwarded, out int dayClaimed)
    {
        coinsAwarded = 0;
        dayClaimed = 0;
        if (!CanClaimToday()) return false;

        dayClaimed = NextRewardDay();
        coinsAwarded = GetRewardForDay(dayClaimed);

        CurrencyManager.AddCoins(coinsAwarded);

        PlayerPrefs.SetString(LastClaimKey, Today());
        PlayerPrefs.SetInt(StreakKey, dayClaimed);
        PlayerPrefs.Save();
        return true;
    }

    private static string Today() => DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);
}

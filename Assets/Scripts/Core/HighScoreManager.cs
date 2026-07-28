using System.Collections.Generic;
using UnityEngine;

public static class HighScoreManager
{
    private const string ScoresKey = "HighScores";
    private const int MaxEntries = 10;

    public static int BestScore
    {
        get
        {
            int[] scores = GetScores();
            return scores.Length > 0 ? scores[0] : 0;
        }
    }

    public static int[] GetScores()
    {
        string raw = PlayerPrefs.GetString(ScoresKey, "");
        if (string.IsNullOrEmpty(raw)) return new int[0];

        string[] parts = raw.Split(',');
        List<int> scores = new List<int>(parts.Length);
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int value)) scores.Add(value);
        }
        return scores.ToArray();
    }

    public static bool SubmitScore(int score)
    {
        List<int> scores = new List<int>(GetScores()) { score };
        scores.Sort((a, b) => b.CompareTo(a));
        if (scores.Count > MaxEntries)
        {
            scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);
        }

        PlayerPrefs.SetString(ScoresKey, string.Join(",", scores));
        PlayerPrefs.Save();

        return scores[0] == score;
    }
}

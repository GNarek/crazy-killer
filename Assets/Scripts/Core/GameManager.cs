using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action GameEnded;

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        Score += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        GameEnded?.Invoke();
    }
}

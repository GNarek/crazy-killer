using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int VictoryCoinBonus = 50;

    public static GameManager Instance { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action GameEnded;
    public event Action<int> GameWon;

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

    private void Start()
    {
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.Victory += Win;
        }
    }

    private void OnDestroy()
    {
        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.Victory -= Win;
        }
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
        CurrencyManager.AddCoins(Score);
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayGameOver();
        GameEnded?.Invoke();
    }

    public void Win()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        int coinsAwarded = Score + VictoryCoinBonus;
        CurrencyManager.AddCoins(coinsAwarded);
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayVictory();
        GameWon?.Invoke(coinsAwarded);
    }
}

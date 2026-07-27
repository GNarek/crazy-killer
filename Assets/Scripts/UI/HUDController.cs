using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Image wallHealthFill;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text coinsEarnedText;
    [SerializeField] private Text waveText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryCoinsText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged += HandleScoreChanged;
            GameManager.Instance.GameEnded += HandleGameOver;
            GameManager.Instance.GameWon += HandleGameWon;
            HandleScoreChanged(GameManager.Instance.Score);
        }

        if (DefenseWall.Instance != null)
        {
            DefenseWall.Instance.HealthChanged += HandleWallHealthChanged;
        }

        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.WaveChanged += HandleWaveChanged;
        }

        gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged -= HandleScoreChanged;
            GameManager.Instance.GameEnded -= HandleGameOver;
            GameManager.Instance.GameWon -= HandleGameWon;
        }

        if (DefenseWall.Instance != null)
        {
            DefenseWall.Instance.HealthChanged -= HandleWallHealthChanged;
        }

        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.WaveChanged -= HandleWaveChanged;
        }
    }

    private void HandleScoreChanged(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void HandleWallHealthChanged(float current, float max)
    {
        wallHealthFill.fillAmount = max > 0f ? current / max : 0f;
    }

    private void HandleWaveChanged(int current, int total)
    {
        if (waveText == null) return;
        waveText.text = current > total ? "Complete!" : $"Wave {current}/{total}";
    }

    private void HandleGameOver()
    {
        if (coinsEarnedText != null && GameManager.Instance != null)
        {
            coinsEarnedText.text = $"+{GameManager.Instance.Score} Coins";
        }
        gameOverPanel.SetActive(true);
    }

    private void HandleGameWon(int coinsAwarded)
    {
        if (victoryCoinsText != null)
        {
            victoryCoinsText.text = $"+{coinsAwarded} Coins";
        }
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }
}

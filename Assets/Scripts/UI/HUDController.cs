using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Image wallHealthFill;
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged += HandleScoreChanged;
            GameManager.Instance.GameEnded += HandleGameOver;
            HandleScoreChanged(GameManager.Instance.Score);
        }

        if (DefenseWall.Instance != null)
        {
            DefenseWall.Instance.HealthChanged += HandleWallHealthChanged;
        }

        gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged -= HandleScoreChanged;
            GameManager.Instance.GameEnded -= HandleGameOver;
        }

        if (DefenseWall.Instance != null)
        {
            DefenseWall.Instance.HealthChanged -= HandleWallHealthChanged;
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

    private void HandleGameOver()
    {
        gameOverPanel.SetActive(true);
    }
}

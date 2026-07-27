using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public float PlayerHealth { get; private set; } = 100f;

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
    }

    public void DamagePlayer(float amount)
    {
        PlayerHealth -= amount;
        if (PlayerHealth <= 0f)
        {
            PlayerHealth = 0f;
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
    }
}

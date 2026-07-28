using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text coinsEarnedText;
    [SerializeField] private Text waveText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryCoinsText;
    [SerializeField] private Text chestBannerText;
    [SerializeField] private Text buffPopupText;

    private Coroutine chestBannerRoutine;
    private Coroutine buffPopupRoutine;
    private Vector2 buffPopupRestPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged += HandleScoreChanged;
            GameManager.Instance.GameEnded += HandleGameOver;
            GameManager.Instance.GameWon += HandleGameWon;
            HandleScoreChanged(GameManager.Instance.Score);
        }

        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.WaveChanged += HandleWaveChanged;
            WaveSpawner.Instance.ChestOpened += HandleChestOpened;
        }

        gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (chestBannerText != null) chestBannerText.gameObject.SetActive(false);

        if (buffPopupText != null)
        {
            buffPopupRestPosition = buffPopupText.rectTransform.anchoredPosition;
            buffPopupText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged -= HandleScoreChanged;
            GameManager.Instance.GameEnded -= HandleGameOver;
            GameManager.Instance.GameWon -= HandleGameWon;
        }

        if (WaveSpawner.Instance != null)
        {
            WaveSpawner.Instance.WaveChanged -= HandleWaveChanged;
            WaveSpawner.Instance.ChestOpened -= HandleChestOpened;
        }
    }

    private void HandleScoreChanged(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void HandleWaveChanged(int current, int total)
    {
        if (waveText == null) return;
        waveText.text = current > total ? "Complete!" : $"Wave {current}/{total}";
    }

    private void HandleChestOpened(int bonus)
    {
        AudioManager.Instance?.PlayChest();

        if (chestBannerText == null) return;
        chestBannerText.text = $"CHEST! +{bonus} COINS";
        if (chestBannerRoutine != null) StopCoroutine(chestBannerRoutine);
        chestBannerRoutine = StartCoroutine(ShowChestBanner());
    }

    private IEnumerator ShowChestBanner()
    {
        chestBannerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        chestBannerText.gameObject.SetActive(false);
    }

    public void ShowBuffPopup(string label, Color color)
    {
        if (buffPopupText == null) return;

        buffPopupText.text = label;
        buffPopupText.color = color;
        if (buffPopupRoutine != null) StopCoroutine(buffPopupRoutine);
        buffPopupRoutine = StartCoroutine(AnimateBuffPopup());
    }

    private IEnumerator AnimateBuffPopup()
    {
        const float duration = 0.9f;
        const float riseDistance = 40f;

        RectTransform rt = buffPopupText.rectTransform;
        Vector2 startPos = buffPopupRestPosition + new Vector2(0f, -riseDistance);
        Color baseColor = buffPopupText.color;

        buffPopupText.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            rt.anchoredPosition = Vector2.Lerp(startPos, buffPopupRestPosition, progress);
            Color c = baseColor;
            c.a = 1f - progress;
            buffPopupText.color = c;
            yield return null;
        }

        buffPopupText.gameObject.SetActive(false);
        rt.anchoredPosition = buffPopupRestPosition;
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

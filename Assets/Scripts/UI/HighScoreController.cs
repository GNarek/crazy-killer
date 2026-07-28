using UnityEngine;
using UnityEngine.UI;

public class HighScoreController : MonoBehaviour
{
    [SerializeField] private Text[] rankTexts;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        int[] scores = HighScoreManager.GetScores();
        for (int i = 0; i < rankTexts.Length; i++)
        {
            rankTexts[i].text = i < scores.Length ? $"{i + 1}. {scores[i]}" : $"{i + 1}. —";
        }
    }
}

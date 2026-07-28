using UnityEngine;
using UnityEngine.UI;

public class DailyRewardController : MonoBehaviour
{
    [SerializeField] private Text[] dayTexts;
    [SerializeField] private Button claimButton;
    [SerializeField] private Text claimButtonLabel;
    [SerializeField] private Text statusText;

    private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.2f);
    private static readonly Color NormalColor = Color.white;

    private void OnEnable()
    {
        Refresh();
    }

    public void Claim()
    {
        if (DailyRewardManager.TryClaim(out int coinsAwarded, out int dayClaimed))
        {
            statusText.text = $"Day {dayClaimed} claimed: +{coinsAwarded} coins!";
        }
        Refresh();
    }

    private void Refresh()
    {
        int highlightDay = DailyRewardManager.NextRewardDay();
        bool canClaim = DailyRewardManager.CanClaimToday();

        for (int i = 0; i < dayTexts.Length; i++)
        {
            int day = i + 1;
            dayTexts[i].text = $"Day {day}\n+{DailyRewardManager.GetRewardForDay(day)}";
            dayTexts[i].color = day == highlightDay ? HighlightColor : NormalColor;
        }

        claimButton.interactable = canClaim;
        claimButtonLabel.text = canClaim ? $"CLAIM +{DailyRewardManager.GetRewardForDay(highlightDay)}" : "COME BACK TOMORROW";

        if (canClaim)
        {
            statusText.text = "";
        }
    }
}

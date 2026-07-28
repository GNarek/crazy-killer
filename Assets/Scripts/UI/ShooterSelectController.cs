using UnityEngine;
using UnityEngine.UI;

public class ShooterSelectController : MonoBehaviour
{
    [SerializeField] private Text coinsText;

    [SerializeField] private Text standardStatusText;
    [SerializeField] private Button standardActionButton;
    [SerializeField] private Text standardActionLabel;

    [SerializeField] private Text rapidStatusText;
    [SerializeField] private Button rapidActionButton;
    [SerializeField] private Text rapidActionLabel;

    [SerializeField] private Text heavyStatusText;
    [SerializeField] private Button heavyActionButton;
    [SerializeField] private Text heavyActionLabel;

    private void OnEnable()
    {
        Refresh();
    }

    public void ActionStandard() => Action(ShooterManager.ShooterType.Standard);
    public void ActionRapid() => Action(ShooterManager.ShooterType.Rapid);
    public void ActionHeavy() => Action(ShooterManager.ShooterType.Heavy);

    private void Action(ShooterManager.ShooterType type)
    {
        if (ShooterManager.IsUnlocked(type))
        {
            ShooterManager.Selected = type;
        }
        else
        {
            ShooterManager.TryUnlock(type);
        }
        Refresh();
    }

    private void Refresh()
    {
        coinsText.text = $"Coins: {CurrencyManager.Coins}";
        RefreshRow(ShooterManager.ShooterType.Standard, standardStatusText, standardActionButton, standardActionLabel);
        RefreshRow(ShooterManager.ShooterType.Rapid, rapidStatusText, rapidActionButton, rapidActionLabel);
        RefreshRow(ShooterManager.ShooterType.Heavy, heavyStatusText, heavyActionButton, heavyActionLabel);
    }

    private static void RefreshRow(ShooterManager.ShooterType type, Text statusText, Button actionButton, Text actionLabel)
    {
        bool unlocked = ShooterManager.IsUnlocked(type);
        bool selected = ShooterManager.Selected == type;

        statusText.text = selected ? "SELECTED" : (unlocked ? "OWNED" : $"{ShooterManager.GetCost(type)} coins");
        actionLabel.text = selected ? "IN USE" : (unlocked ? "SELECT" : "UNLOCK");
        actionButton.interactable = !selected && (unlocked || CurrencyManager.Coins >= ShooterManager.GetCost(type));
    }
}

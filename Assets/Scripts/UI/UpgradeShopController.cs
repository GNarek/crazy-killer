using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopController : MonoBehaviour
{
    [SerializeField] private Text coinsText;
    [SerializeField] private Text damageLevelText;
    [SerializeField] private Text damageCostText;
    [SerializeField] private Button damageBuyButton;
    [SerializeField] private Text fireRateLevelText;
    [SerializeField] private Text fireRateCostText;
    [SerializeField] private Button fireRateBuyButton;
    [SerializeField] private Text wallHealthLevelText;
    [SerializeField] private Text wallHealthCostText;
    [SerializeField] private Button wallHealthBuyButton;

    private void OnEnable()
    {
        Refresh();
    }

    public void BuyDamage() => Purchase(UpgradeManager.UpgradeType.Damage);
    public void BuyFireRate() => Purchase(UpgradeManager.UpgradeType.FireRate);
    public void BuyWallHealth() => Purchase(UpgradeManager.UpgradeType.WallHealth);

    private void Purchase(UpgradeManager.UpgradeType type)
    {
        UpgradeManager.TryPurchase(type);
        Refresh();
    }

    private void Refresh()
    {
        coinsText.text = $"Coins: {CurrencyManager.Coins}";
        RefreshRow(UpgradeManager.UpgradeType.Damage, damageLevelText, damageCostText, damageBuyButton);
        RefreshRow(UpgradeManager.UpgradeType.FireRate, fireRateLevelText, fireRateCostText, fireRateBuyButton);
        RefreshRow(UpgradeManager.UpgradeType.WallHealth, wallHealthLevelText, wallHealthCostText, wallHealthBuyButton);
    }

    private static void RefreshRow(UpgradeManager.UpgradeType type, Text levelText, Text costText, Button buyButton)
    {
        int level = UpgradeManager.GetLevel(type);
        bool maxed = UpgradeManager.IsMaxLevel(type);
        levelText.text = $"Lv {level}";
        costText.text = maxed ? "MAX" : UpgradeManager.GetCost(type).ToString();
        buyButton.interactable = !maxed && CurrencyManager.Coins >= UpgradeManager.GetCost(type);
    }
}

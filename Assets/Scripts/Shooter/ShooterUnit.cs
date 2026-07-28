using UnityEngine;

[RequireComponent(typeof(Weapon), typeof(ShooterController))]
public class ShooterUnit : MonoBehaviour
{
    public ShooterManager.ShooterType Type { get; private set; }
    public int Tier { get; private set; }
    public int SlotIndex { get; set; } = -1;

    private Weapon weapon;
    private ShooterController controller;
    private Renderer unitRenderer;

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
        controller = GetComponent<ShooterController>();
        unitRenderer = GetComponent<Renderer>();
    }

    public void Initialize(ShooterManager.ShooterType type, int tier)
    {
        Type = type;
        Tier = tier;

        weapon.Configure(type, tier);
        controller.Configure(type);

        if (unitRenderer != null)
        {
            unitRenderer.material.color = ShooterManager.GetColor(type);
        }

        float tierScale = ShooterManager.GetTierScaleMultiplier(tier);
        transform.localScale = ShooterManager.GetScale(type) * tierScale;
    }

    public bool CanMergeWith(ShooterUnit other)
    {
        return other != null && other != this && other.Type == Type && other.Tier == Tier && Tier < ShooterManager.MaxTier;
    }
}

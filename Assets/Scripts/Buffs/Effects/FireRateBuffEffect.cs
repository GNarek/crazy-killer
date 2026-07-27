using UnityEngine;

public class FireRateBuffEffect : IBuffEffect
{
    private readonly float amount;

    public FireRateBuffEffect(float amount)
    {
        this.amount = amount;
    }

    public void Apply(GameObject target)
    {
        if (target.TryGetComponent(out ShooterController shooter))
            shooter.AddFireRateBonus(amount);
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out ShooterController shooter))
            shooter.RemoveFireRateBonus(amount);
    }
}

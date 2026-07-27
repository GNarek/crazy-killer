using UnityEngine;

public class HealthBuffEffect : IBuffEffect
{
    private readonly float amount;

    public HealthBuffEffect(float amount)
    {
        this.amount = amount;
    }

    public void Apply(GameObject target)
    {
        if (target.TryGetComponent(out Health health))
            health.AddMaxHealth(amount);
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out Health health))
            health.RemoveMaxHealth(amount);
    }
}

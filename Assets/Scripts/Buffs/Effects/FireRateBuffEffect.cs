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
        SquadManager.Instance?.AddFireRateBonus(amount);
    }

    public void Remove(GameObject target)
    {
        SquadManager.Instance?.RemoveFireRateBonus(amount);
    }
}

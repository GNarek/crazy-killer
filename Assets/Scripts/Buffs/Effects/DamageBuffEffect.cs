using UnityEngine;

public class DamageBuffEffect : IBuffEffect
{
    private readonly float amount;

    public DamageBuffEffect(float amount)
    {
        this.amount = amount;
    }

    public void Apply(GameObject target)
    {
        SquadManager.Instance?.AddDamageBonus(amount);
    }

    public void Remove(GameObject target)
    {
        SquadManager.Instance?.RemoveDamageBonus(amount);
    }
}

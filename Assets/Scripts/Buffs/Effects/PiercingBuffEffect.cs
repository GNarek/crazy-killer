using UnityEngine;

public class PiercingBuffEffect : IBuffEffect
{
    private readonly int amount;

    public PiercingBuffEffect(float amount)
    {
        this.amount = Mathf.RoundToInt(amount);
    }

    public void Apply(GameObject target)
    {
        SquadManager.Instance?.AddPierceBonus(amount);
    }

    public void Remove(GameObject target)
    {
        SquadManager.Instance?.RemovePierceBonus(amount);
    }
}

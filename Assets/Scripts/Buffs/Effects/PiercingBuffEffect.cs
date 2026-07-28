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
        if (target.TryGetComponent(out Weapon weapon))
            weapon.AddPierce(amount);
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out Weapon weapon))
            weapon.RemovePierce(amount);
    }
}

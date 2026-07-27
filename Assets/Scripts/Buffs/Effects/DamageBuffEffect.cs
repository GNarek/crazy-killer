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
        if (target.TryGetComponent(out Weapon weapon))
            weapon.Damage += amount;
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out Weapon weapon))
            weapon.Damage -= amount;
    }
}

using UnityEngine;

public class MultiShotBuffEffect : IBuffEffect
{
    private readonly int amount;

    public MultiShotBuffEffect(float amount)
    {
        this.amount = Mathf.RoundToInt(amount);
    }

    public void Apply(GameObject target)
    {
        if (target.TryGetComponent(out Weapon weapon))
            weapon.AddExtraShots(amount);
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out Weapon weapon))
            weapon.RemoveExtraShots(amount);
    }
}

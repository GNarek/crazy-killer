using UnityEngine;

public class MoveSpeedBuffEffect : IBuffEffect
{
    private readonly float amount;

    public MoveSpeedBuffEffect(float amount)
    {
        this.amount = amount;
    }

    public void Apply(GameObject target)
    {
        if (target.TryGetComponent(out LaneMover mover))
            mover.speed += amount;
    }

    public void Remove(GameObject target)
    {
        if (target.TryGetComponent(out LaneMover mover))
            mover.speed -= amount;
    }
}

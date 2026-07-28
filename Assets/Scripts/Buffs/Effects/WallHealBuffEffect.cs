using UnityEngine;

public class WallHealBuffEffect : IBuffEffect
{
    private readonly float amount;

    public WallHealBuffEffect(float amount)
    {
        this.amount = amount;
    }

    public void Apply(GameObject target)
    {
        DefenseWall.Instance?.Heal(amount);
    }

    public void Remove(GameObject target) { }
}

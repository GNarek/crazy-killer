using UnityEngine;

public class ShieldBuffEffect : IBuffEffect
{
    public void Apply(GameObject target)
    {
        DefenseWall.Instance?.SetInvulnerable(true);
    }

    public void Remove(GameObject target)
    {
        DefenseWall.Instance?.SetInvulnerable(false);
    }
}

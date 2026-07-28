using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffReceiver : MonoBehaviour
{
    public event Action<BuffDefinition> BuffApplied;

    private readonly List<BuffDefinition> activeBuffs = new List<BuffDefinition>();

    public void ApplyBuff(BuffDefinition buff)
    {
        activeBuffs.Add(buff);
        GetEffect(buff).Apply(gameObject);
        BuffApplied?.Invoke(buff);

        if (buff.duration > 0f)
        {
            StartCoroutine(RemoveAfterDelay(buff));
        }
    }

    private IEnumerator RemoveAfterDelay(BuffDefinition buff)
    {
        yield return new WaitForSeconds(buff.duration);
        RemoveBuff(buff);
    }

    private void RemoveBuff(BuffDefinition buff)
    {
        if (activeBuffs.Remove(buff))
        {
            GetEffect(buff).Remove(gameObject);
        }
    }

    private IBuffEffect GetEffect(BuffDefinition buff)
    {
        return buff.type switch
        {
            BuffType.Damage => new DamageBuffEffect(buff.value),
            BuffType.FireRate => new FireRateBuffEffect(buff.value),
            BuffType.MoveSpeed => new MoveSpeedBuffEffect(buff.value),
            BuffType.WallHeal => new WallHealBuffEffect(buff.value),
            BuffType.MultiShot => new MultiShotBuffEffect(buff.value),
            BuffType.Piercing => new PiercingBuffEffect(buff.value),
            BuffType.Shield => new ShieldBuffEffect(),
            _ => new NullBuffEffect()
        };
    }
}

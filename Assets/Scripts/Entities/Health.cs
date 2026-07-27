using System;
using UnityEngine;

public class Health : MonoBehaviour, IPoolable
{
    public event Action Died;

    [SerializeField] private float maxHealth = 10f;
    public float MaxHealth => maxHealth;
    public float Current { get; private set; }

    public void SetMax(float value, bool resetCurrent = true)
    {
        maxHealth = value;
        if (resetCurrent) Current = maxHealth;
    }

    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
        Current += amount;
    }

    public void RemoveMaxHealth(float amount)
    {
        maxHealth -= amount;
        Current = Mathf.Min(Current, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (Current <= 0f) return;
        Current -= amount;
        if (Current <= 0f)
        {
            Current = 0f;
            Died?.Invoke();
        }
    }

    public void OnSpawn() => Current = maxHealth;
    public void OnDespawn() { }
}

using System;
using UnityEngine;

public class Health : MonoBehaviour, IPoolable
{
    public event Action Died;
    public event Action<float, float> HealthChanged;
    public event Action<float> DamageTaken;

    [SerializeField] private float maxHealth = 10f;
    public float MaxHealth => maxHealth;
    public float Current { get; private set; }

    public void SetMax(float value, bool resetCurrent = true)
    {
        maxHealth = value;
        SetCurrent(resetCurrent ? maxHealth : Current);
    }

    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
        SetCurrent(Current + amount);
    }

    public void RemoveMaxHealth(float amount)
    {
        maxHealth -= amount;
        SetCurrent(Current);
    }

    public void Heal(float amount)
    {
        SetCurrent(Current + amount);
    }

    public void TakeDamage(float amount)
    {
        if (Current <= 0f) return;

        DamageTaken?.Invoke(amount);
        SetCurrent(Current - amount);

        if (Current <= 0f)
        {
            Died?.Invoke();
        }
    }

    private void SetCurrent(float value)
    {
        Current = Mathf.Clamp(value, 0f, maxHealth);
        HealthChanged?.Invoke(Current, maxHealth);
    }

    public void OnSpawn() => SetCurrent(maxHealth);
    public void OnDespawn() { }
}

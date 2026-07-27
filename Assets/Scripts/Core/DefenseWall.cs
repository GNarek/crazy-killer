using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DefenseWall : MonoBehaviour
{
    public static DefenseWall Instance { get; private set; }

    private Health health;

    public event Action<float, float> HealthChanged
    {
        add => health.HealthChanged += value;
        remove => health.HealthChanged -= value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        health = GetComponent<Health>();
        health.Died += HandleDestroyed;
        health.OnSpawn();
    }

    public void TakeHit(float amount)
    {
        health.TakeDamage(amount);
        CameraShake.Instance?.Shake(0.15f, 0.12f);
    }

    private void HandleDestroyed()
    {
        GameManager.Instance?.GameOver();
    }
}

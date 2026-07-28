using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DefenseWall : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public static DefenseWall Instance { get; private set; }

    public bool IsInvulnerable { get; private set; }

    private Health health;
    private Renderer wallRenderer;
    private MaterialPropertyBlock propertyBlock;

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
        wallRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        health.Died += HandleDestroyed;
        health.DamageTaken += HandleDamageTaken;
        health.SetMax(health.MaxHealth + UpgradeManager.GetWallHealthBonus());
    }

    public void TakeHit(float amount)
    {
        if (IsInvulnerable) return;
        health.TakeDamage(amount);
    }

    public void Heal(float amount)
    {
        health.Heal(amount);
    }

    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
        if (wallRenderer == null) return;

        wallRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, value ? new Color(0.3f, 0.6f, 1f) : wallRenderer.sharedMaterial.color);
        wallRenderer.SetPropertyBlock(propertyBlock);
    }

    private void HandleDamageTaken(float amount)
    {
        AudioManager.Instance?.PlayWallHit();
        CameraShake.Instance?.Shake(0.15f, 0.12f);
        Haptics.Pulse();
    }

    private void HandleDestroyed()
    {
        GameManager.Instance?.GameOver();
    }
}

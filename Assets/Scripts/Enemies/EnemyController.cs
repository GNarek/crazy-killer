using UnityEngine;

[RequireComponent(typeof(Health), typeof(LaneMover))]
public class EnemyController : MonoBehaviour, IPoolable
{
    [SerializeField] private float goalZ = 0f;

    private float damage;
    private EnemyDefinition definition;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.Died += HandleDeath;
    }

    public void Initialize(EnemyDefinition def)
    {
        definition = def;
        damage = def.damage;
        health.SetMax(def.maxHealth);
        GetComponent<LaneMover>().speed = def.moveSpeed;
    }

    private void Update()
    {
        if (transform.position.z <= goalZ)
        {
            ReachGoal();
        }
    }

    private void ReachGoal()
    {
        GameManager.Instance?.DamagePlayer(damage);
        Despawn();
    }

    private void HandleDeath()
    {
        GameManager.Instance?.AddScore(definition != null ? definition.scoreValue : 1);
        Despawn();
    }

    private void Despawn()
    {
        PoolManager.Instance.Despawn(gameObject);
    }

    public void OnSpawn() { }
    public void OnDespawn() { }
}

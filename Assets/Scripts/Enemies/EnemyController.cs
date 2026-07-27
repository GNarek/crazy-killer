using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(LaneMover))]
public class EnemyController : MonoBehaviour, IPoolable
{
    public static event Action AnyDespawned;

    [SerializeField] private float goalZ = 0f;
    [SerializeField] private float deathDelay = 0.15f;

    private float damage;
    private EnemyDefinition definition;
    private Health health;
    private Collider hitCollider;
    private LaneMover mover;

    private void Awake()
    {
        health = GetComponent<Health>();
        hitCollider = GetComponent<Collider>();
        mover = GetComponent<LaneMover>();
        health.Died += HandleDeath;
    }

    public void Initialize(EnemyDefinition def, float statMultiplier = 1f)
    {
        definition = def;
        damage = def.damage * statMultiplier;
        health.SetMax(def.maxHealth * statMultiplier);
        mover.speed = def.moveSpeed * Mathf.Sqrt(statMultiplier);
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
        DefenseWall.Instance?.TakeHit(damage);
        Despawn();
    }

    private void HandleDeath()
    {
        GameManager.Instance?.AddScore(definition != null ? definition.scoreValue : 1);
        hitCollider.enabled = false;
        mover.enabled = false;
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(deathDelay);
        Despawn();
    }

    private void Despawn()
    {
        PoolManager.Instance.Despawn(gameObject);
        AnyDespawned?.Invoke();
    }

    public void OnSpawn()
    {
        hitCollider.enabled = true;
        mover.enabled = true;
    }

    public void OnDespawn() { }
}

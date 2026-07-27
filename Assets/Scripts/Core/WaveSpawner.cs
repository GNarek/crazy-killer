using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyDefinition> enemyPool;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1.5f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f || enemyPool.Count == 0) return;

        SpawnRandom();
        timer = spawnInterval;
    }

    private void SpawnRandom()
    {
        EnemyDefinition definition = enemyPool[Random.Range(0, enemyPool.Count)];
        GameObject instance = PoolManager.Instance.Spawn(definition.prefab, spawnPoint.position, spawnPoint.rotation);
        if (instance.TryGetComponent(out EnemyController enemy))
        {
            enemy.Initialize(definition);
        }
    }
}

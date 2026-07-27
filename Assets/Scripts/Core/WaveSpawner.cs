using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyDefinition> enemyPool;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.4f;
    [SerializeField] private float difficultyRampSeconds = 90f;
    [SerializeField] private float maxStatMultiplier = 2f;
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;

    private float timer;
    private float elapsed;

    private void Update()
    {
        elapsed += Time.deltaTime;
        timer -= Time.deltaTime;
        if (timer > 0f || enemyPool.Count == 0) return;

        SpawnRandom();
        timer = CurrentSpawnInterval();
    }

    private float DifficultyProgress()
    {
        return Mathf.Clamp01(elapsed / difficultyRampSeconds);
    }

    private float CurrentSpawnInterval()
    {
        return Mathf.Lerp(spawnInterval, minSpawnInterval, DifficultyProgress());
    }

    private float CurrentStatMultiplier()
    {
        return Mathf.Lerp(1f, maxStatMultiplier, DifficultyProgress());
    }

    private void SpawnRandom()
    {
        EnemyDefinition definition = enemyPool[Random.Range(0, enemyPool.Count)];
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x = Random.Range(minX, maxX);
        GameObject instance = PoolManager.Instance.Spawn(definition.prefab, spawnPosition, spawnPoint.rotation);
        if (instance.TryGetComponent(out EnemyController enemy))
        {
            enemy.Initialize(definition, CurrentStatMultiplier());
        }
    }
}

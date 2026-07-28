using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [SerializeField] private List<EnemyDefinition> enemyPool;
    [SerializeField] private EnemyDefinition bossDefinition;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.6f;
    [SerializeField] private float maxStatMultiplier = 2f;
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;
    [SerializeField] private int maxConcurrentEnemies = 10;
    [SerializeField] private int totalWaves = 15;
    [SerializeField] private int bossEveryNWaves = 5;
    [SerializeField] private float waveDuration = 15f;
    [SerializeField] private float bossWaveDuration = 30f;
    [SerializeField] private float intermissionDuration = 4f;
    [SerializeField] private int chestEveryNWaves = 3;

    public event Action<int, int> WaveChanged;
    public event Action Victory;
    public event Action<int> ChestOpened;

    private float timer;
    private float waveTimer;
    private int activeCount;
    private int currentWave;
    private bool inIntermission;
    private bool bossAlive;
    private bool completed;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        EnemyController.AnyDespawned += HandleEnemyDespawned;
    }

    private void OnDisable()
    {
        EnemyController.AnyDespawned -= HandleEnemyDespawned;
    }

    private void Start()
    {
        StartNextWave();
    }

    private void Update()
    {
        if (completed) return;

        if (inIntermission)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f) StartNextWave();
            return;
        }

        bool isBossWave = IsBossWave(currentWave);
        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f || (isBossWave && !bossAlive))
        {
            EndWave();
            return;
        }

        if (isBossWave) return;

        timer -= Time.deltaTime;
        if (timer > 0f || enemyPool.Count == 0 || activeCount >= maxConcurrentEnemies) return;

        SpawnRandom();
        timer = CurrentSpawnInterval();
    }

    private bool IsBossWave(int wave)
    {
        return bossDefinition != null && wave % bossEveryNWaves == 0;
    }

    private void StartNextWave()
    {
        currentWave++;
        inIntermission = false;
        WaveChanged?.Invoke(currentWave, totalWaves);

        if (currentWave > totalWaves)
        {
            completed = true;
            Victory?.Invoke();
            return;
        }

        if (IsBossWave(currentWave))
        {
            waveTimer = bossWaveDuration;
            SpawnBoss();
        }
        else
        {
            waveTimer = waveDuration;
            timer = 0f;
        }
    }

    private void EndWave()
    {
        inIntermission = true;
        waveTimer = intermissionDuration;

        if (currentWave % chestEveryNWaves == 0)
        {
            int bonus = 10 + currentWave * 2;
            GameManager.Instance?.AddScore(bonus);
            ChestOpened?.Invoke(bonus);
        }
    }

    private float WaveProgress()
    {
        return totalWaves > 1 ? Mathf.Clamp01((float)(currentWave - 1) / (totalWaves - 1)) : 0f;
    }

    private float CurrentSpawnInterval()
    {
        return Mathf.Lerp(spawnInterval, minSpawnInterval, WaveProgress());
    }

    private float CurrentStatMultiplier()
    {
        return Mathf.Lerp(1f, maxStatMultiplier, WaveProgress());
    }

    private void SpawnRandom()
    {
        EnemyDefinition definition = enemyPool[UnityEngine.Random.Range(0, enemyPool.Count)];
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x = UnityEngine.Random.Range(minX, maxX);
        GameObject instance = PoolManager.Instance.Spawn(definition.prefab, spawnPosition, spawnPoint.rotation);
        if (instance.TryGetComponent(out EnemyController enemy))
        {
            enemy.Initialize(definition, CurrentStatMultiplier());
        }
        activeCount++;
    }

    private void SpawnBoss()
    {
        GameObject instance = PoolManager.Instance.Spawn(bossDefinition.prefab, spawnPoint.position, spawnPoint.rotation);
        if (instance.TryGetComponent(out EnemyController enemy))
        {
            enemy.Initialize(bossDefinition, CurrentStatMultiplier());
        }
        if (instance.TryGetComponent(out Health health))
        {
            health.Died -= HandleBossDeath;
            health.Died += HandleBossDeath;
        }
        activeCount++;
        bossAlive = true;
    }

    private void HandleBossDeath()
    {
        bossAlive = false;
    }

    private void HandleEnemyDespawned()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> pickupPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 6f;
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f || pickupPrefabs.Count == 0) return;

        SpawnRandom();
        timer = spawnInterval;
    }

    private void SpawnRandom()
    {
        GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Count)];
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x = Random.Range(minX, maxX);
        PoolManager.Instance.Spawn(prefab, spawnPosition, spawnPoint.rotation);
    }
}

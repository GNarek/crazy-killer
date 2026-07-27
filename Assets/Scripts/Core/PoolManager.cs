using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool pool))
        {
            pool = new ObjectPool(prefab, transform);
            pools[prefab] = pool;
        }

        GameObject instance = pool.Get(position, rotation);
        if (!instance.TryGetComponent(out PoolTag tag))
            tag = instance.AddComponent<PoolTag>();
        tag.SourcePrefab = prefab;
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance.TryGetComponent(out PoolTag tag) && pools.TryGetValue(tag.SourcePrefab, out ObjectPool pool))
        {
            pool.Release(instance);
        }
        else
        {
            Destroy(instance);
        }
    }
}

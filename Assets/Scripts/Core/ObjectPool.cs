using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Stack<GameObject> inactive = new Stack<GameObject>();

    public ObjectPool(GameObject prefab, Transform parent, int prewarm = 0)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < prewarm; i++)
        {
            GameObject obj = CreateNew();
            obj.SetActive(false);
            inactive.Push(obj);
        }
    }

    private GameObject CreateNew()
    {
        return Object.Instantiate(prefab, parent);
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = inactive.Count > 0 ? inactive.Pop() : CreateNew();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        foreach (IPoolable poolable in obj.GetComponents<IPoolable>())
        {
            poolable.OnSpawn();
        }
        return obj;
    }

    public void Release(GameObject obj)
    {
        foreach (IPoolable poolable in obj.GetComponents<IPoolable>())
        {
            poolable.OnDespawn();
        }
        obj.SetActive(false);
        inactive.Push(obj);
    }
}

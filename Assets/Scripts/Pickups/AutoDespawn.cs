using UnityEngine;

public class AutoDespawn : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 12f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }

    public void OnSpawn() => timer = 0f;
    public void OnDespawn() { }
}

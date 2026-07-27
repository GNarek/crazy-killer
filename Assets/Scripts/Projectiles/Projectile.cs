using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask hittableLayers;

    private Vector3 direction;
    private float damage;
    private float speed;
    private float timer;

    public void Launch(Vector3 travelDirection, float dmg, float projectileSpeed)
    {
        direction = travelDirection.normalized;
        damage = dmg;
        speed = projectileSpeed;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Despawn();
            return;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hittableLayers.value) == 0) return;

        if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
            AudioManager.Instance?.PlayHit();
        }
        Despawn();
    }

    private void Despawn()
    {
        PoolManager.Instance.Despawn(gameObject);
    }

    public void OnSpawn() { }
    public void OnDespawn() { }
}

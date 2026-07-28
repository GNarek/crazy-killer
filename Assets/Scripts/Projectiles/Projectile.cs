using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask hittableLayers;

    private Vector3 direction;
    private float damage;
    private float speed;
    private float timer;
    private int remainingPierces;

    public void Launch(Vector3 travelDirection, float dmg, float projectileSpeed, int pierceCount = 0)
    {
        direction = travelDirection.normalized;
        damage = dmg;
        speed = projectileSpeed;
        timer = 0f;
        remainingPierces = pierceCount;
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

        if (other.TryGetComponent(out DefenseWall wall))
        {
            wall.TakeHit(damage);
        }
        else if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
            AudioManager.Instance?.PlayHit();
        }

        if (remainingPierces > 0)
        {
            remainingPierces--;
            return;
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

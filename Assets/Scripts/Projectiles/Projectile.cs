using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 3f;

    private Transform target;
    private float damage;
    private float speed;
    private float timer;

    public void Launch(Transform newTarget, float dmg, float projectileSpeed)
    {
        target = newTarget;
        damage = dmg;
        speed = projectileSpeed;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime || target == null)
        {
            Despawn();
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        float step = speed * Time.deltaTime;

        if (toTarget.sqrMagnitude <= step * step)
        {
            HitTarget();
            return;
        }

        transform.position += toTarget.normalized * step;
    }

    private void HitTarget()
    {
        if (target != null && target.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }
        Despawn();
    }

    private void Despawn()
    {
        target = null;
        PoolManager.Instance.Despawn(gameObject);
    }

    public void OnSpawn() { }
    public void OnDespawn() { }
}

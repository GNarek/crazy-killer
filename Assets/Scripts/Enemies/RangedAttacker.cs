using UnityEngine;

[RequireComponent(typeof(LaneMover), typeof(Health))]
public class RangedAttacker : MonoBehaviour
{
    [SerializeField] private float stopZ = 4f;
    [SerializeField] private float fireRate = 0.7f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 10f;

    private LaneMover mover;
    private float cooldown;
    private bool stopped;
    private bool dead;

    private void Awake()
    {
        mover = GetComponent<LaneMover>();
        GetComponent<Health>().Died += HandleDeath;
    }

    private void OnEnable()
    {
        stopped = false;
        cooldown = 0f;
        dead = false;
    }

    private void Update()
    {
        if (dead) return;

        if (!stopped)
        {
            if (transform.position.z <= stopZ)
            {
                stopped = true;
                mover.enabled = false;
            }
            return;
        }

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        Fire();
        cooldown = 1f / fireRate;
    }

    private void Fire()
    {
        GameObject projectileObj = PoolManager.Instance.Spawn(projectilePrefab, transform.position, Quaternion.identity);
        if (projectileObj.TryGetComponent(out Projectile projectile))
        {
            projectile.Launch(Vector3.back, damage, projectileSpeed);
        }
    }

    private void HandleDeath()
    {
        dead = true;
    }
}

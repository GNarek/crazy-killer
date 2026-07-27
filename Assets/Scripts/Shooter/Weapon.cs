using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 12f;

    public float Damage { get => damage; set => damage = value; }

    public void Fire(Transform origin, Transform target)
    {
        GameObject projectileObj = PoolManager.Instance.Spawn(projectilePrefab, origin.position, origin.rotation);
        if (projectileObj.TryGetComponent(out Projectile projectile))
        {
            projectile.Launch(target, damage, projectileSpeed);
        }
    }
}

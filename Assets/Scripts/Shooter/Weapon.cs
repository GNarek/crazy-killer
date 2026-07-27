using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float spreadAngle = 25f;

    private int extraShots;

    public float Damage { get => damage; set => damage = value; }
    public int ShotCount => 1 + extraShots;

    public void AddExtraShots(int amount) => extraShots += amount;
    public void RemoveExtraShots(int amount) => extraShots = Mathf.Max(0, extraShots - amount);

    public void Fire(Transform origin)
    {
        int shotCount = ShotCount;
        AudioManager.Instance?.PlayShoot();

        for (int i = 0; i < shotCount; i++)
        {
            float angle = shotCount == 1 ? 0f : -spreadAngle / 2f + i * (spreadAngle / (shotCount - 1));
            Quaternion rotation = origin.rotation * Quaternion.Euler(0f, angle, 0f);

            GameObject projectileObj = PoolManager.Instance.Spawn(projectilePrefab, origin.position, rotation);
            if (projectileObj.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(rotation * Vector3.forward, damage, projectileSpeed);
            }
        }
    }
}

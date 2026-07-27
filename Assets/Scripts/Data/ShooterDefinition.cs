using UnityEngine;

[CreateAssetMenu(fileName = "ShooterDefinition", menuName = "CrazyKiller/Shooter Definition")]
public class ShooterDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;
    public float fireRate = 1f;
    public float damage = 1f;
    public float range = 6f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
}

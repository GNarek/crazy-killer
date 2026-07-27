using UnityEngine;

[RequireComponent(typeof(Weapon), typeof(Targeting))]
public class ShooterController : MonoBehaviour
{
    [SerializeField] private float baseFireRate = 1f;

    private float fireRateBonus;
    private Weapon weapon;
    private Targeting targeting;
    private float cooldown;

    private float EffectiveFireRate => Mathf.Max(0.01f, baseFireRate + fireRateBonus);

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
        targeting = GetComponent<Targeting>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        Transform target = targeting.FindClosest();
        if (target == null) return;

        weapon.Fire(transform, target);
        cooldown = 1f / EffectiveFireRate;
    }

    public void AddFireRateBonus(float amount) => fireRateBonus += amount;
    public void RemoveFireRateBonus(float amount) => fireRateBonus -= amount;
}

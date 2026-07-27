using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class ShooterController : MonoBehaviour
{
    [SerializeField] private float baseFireRate = 1f;

    private float fireRateBonus;
    private Weapon weapon;
    private float cooldown;

    private float EffectiveFireRate => Mathf.Max(0.01f, baseFireRate + fireRateBonus);

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        weapon.Fire(transform);
        cooldown = 1f / EffectiveFireRate;
    }

    public void AddFireRateBonus(float amount) => fireRateBonus += amount;
    public void RemoveFireRateBonus(float amount) => fireRateBonus -= amount;
}

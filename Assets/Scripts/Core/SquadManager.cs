using System;
using System.Collections.Generic;
using UnityEngine;

public class SquadManager : MonoBehaviour
{
    public static SquadManager Instance { get; private set; }

    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private float[] slotPositions = { -2.4f, -1.2f, 0f, 1.2f, 2.4f };
    [SerializeField] private float unitY = 0.5f;
    [SerializeField] private float unitZ = -2f;

    private readonly ShooterUnit[] slots = new ShooterUnit[5];

    private float damageBonus;
    private float fireRateBonus;
    private int pierceBonus;

    public IEnumerable<ShooterUnit> ActiveUnits
    {
        get
        {
            foreach (ShooterUnit unit in slots)
            {
                if (unit != null) yield return unit;
            }
        }
    }

    public bool IsSquadFull => FindEmptySlot() < 0;
    public int SlotCount => slots.Length;
    public float SlotPosition(int index) => slotPositions[index];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnUnit(ShooterManager.Selected, 1, slots.Length / 2);
    }

    public bool SpawnUnit(ShooterManager.ShooterType type, int tier)
    {
        int slot = FindEmptySlot();
        if (slot < 0) return false;
        SpawnUnit(type, tier, slot);
        return true;
    }

    private void SpawnUnit(ShooterManager.ShooterType type, int tier, int slotIndex)
    {
        Vector3 position = new Vector3(slotPositions[slotIndex], unitY, unitZ);
        GameObject instance = Instantiate(unitPrefab, position, Quaternion.identity);

        if (!instance.TryGetComponent(out ShooterUnit unit)) return;

        unit.Initialize(type, tier);
        unit.SlotIndex = slotIndex;
        slots[slotIndex] = unit;

        if (instance.TryGetComponent(out Weapon weapon))
        {
            weapon.Damage += damageBonus;
            weapon.AddPierce(pierceBonus);
        }
        if (instance.TryGetComponent(out ShooterController controller))
        {
            controller.AddFireRateBonus(fireRateBonus);
        }
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) return i;
        }
        return -1;
    }

    public int FindNearestEmptySlot(float worldX)
    {
        int best = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) continue;
            float dist = Mathf.Abs(slotPositions[i] - worldX);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }

        return best;
    }

    public ShooterUnit GetClosestUnit(float worldX, float maxDistance, ShooterUnit exclude = null)
    {
        ShooterUnit closest = null;
        float closestDist = maxDistance;

        foreach (ShooterUnit unit in ActiveUnits)
        {
            if (unit == exclude) continue;

            float dist = Mathf.Abs(unit.transform.position.x - worldX);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = unit;
            }
        }

        return closest;
    }

    public void FreeSlot(ShooterUnit unit)
    {
        if (unit.SlotIndex >= 0 && unit.SlotIndex < slots.Length && slots[unit.SlotIndex] == unit)
        {
            slots[unit.SlotIndex] = null;
        }
        unit.SlotIndex = -1;
    }

    public void PlaceInSlot(ShooterUnit unit, int slotIndex)
    {
        slots[slotIndex] = unit;
        unit.SlotIndex = slotIndex;

        Vector3 pos = unit.transform.position;
        pos.x = slotPositions[slotIndex];
        unit.transform.position = pos;
    }

    public bool TryMerge(ShooterUnit dragged, ShooterUnit target)
    {
        if (!dragged.CanMergeWith(target)) return false;

        int targetSlot = target.SlotIndex;
        ShooterManager.ShooterType type = target.Type;
        int newTier = target.Tier + 1;

        FreeSlot(dragged);
        FreeSlot(target);
        Destroy(dragged.gameObject);
        Destroy(target.gameObject);

        SpawnUnit(type, newTier, targetSlot);

        AudioManager.Instance?.PlayMerge();
        ParticleFX.PickupSparkle(new Vector3(slotPositions[targetSlot], unitY, unitZ));

        return true;
    }

    public void AddDamageBonus(float amount)
    {
        damageBonus += amount;
        ApplyToWeapons(w => w.Damage += amount);
    }

    public void RemoveDamageBonus(float amount)
    {
        damageBonus -= amount;
        ApplyToWeapons(w => w.Damage -= amount);
    }

    public void AddFireRateBonus(float amount)
    {
        fireRateBonus += amount;
        ApplyToControllers(c => c.AddFireRateBonus(amount));
    }

    public void RemoveFireRateBonus(float amount)
    {
        fireRateBonus -= amount;
        ApplyToControllers(c => c.RemoveFireRateBonus(amount));
    }

    public void AddPierceBonus(int amount)
    {
        pierceBonus += amount;
        ApplyToWeapons(w => w.AddPierce(amount));
    }

    public void RemovePierceBonus(int amount)
    {
        pierceBonus -= amount;
        ApplyToWeapons(w => w.RemovePierce(amount));
    }

    private void ApplyToWeapons(Action<Weapon> action)
    {
        foreach (ShooterUnit unit in ActiveUnits)
        {
            if (unit.TryGetComponent(out Weapon weapon)) action(weapon);
        }
    }

    private void ApplyToControllers(Action<ShooterController> action)
    {
        foreach (ShooterUnit unit in ActiveUnits)
        {
            if (unit.TryGetComponent(out ShooterController controller)) action(controller);
        }
    }
}

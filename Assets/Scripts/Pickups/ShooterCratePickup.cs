using System.Collections.Generic;
using UnityEngine;

public class ShooterCratePickup : MonoBehaviour
{
    private const int FallbackCoins = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out BuffReceiver _)) return;

        ShooterManager.ShooterType type = PickRandomUnlockedType();
        bool spawned = SquadManager.Instance != null && SquadManager.Instance.SpawnUnit(type, 1);

        if (!spawned)
        {
            CurrencyManager.AddCoins(FallbackCoins);
        }

        Vector3 position = transform.position;
        PoolManager.Instance.Despawn(gameObject);

        AudioManager.Instance?.PlayPickup();
        ParticleFX.PickupSparkle(position);
    }

    private static ShooterManager.ShooterType PickRandomUnlockedType()
    {
        List<ShooterManager.ShooterType> unlocked = new List<ShooterManager.ShooterType>();
        foreach (ShooterManager.ShooterType type in System.Enum.GetValues(typeof(ShooterManager.ShooterType)))
        {
            if (ShooterManager.IsUnlocked(type)) unlocked.Add(type);
        }
        return unlocked[Random.Range(0, unlocked.Count)];
    }
}

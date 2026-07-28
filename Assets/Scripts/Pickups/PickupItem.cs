using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private BuffDefinition buff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BuffReceiver receiver))
        {
            receiver.ApplyBuff(buff);
            Vector3 position = transform.position;
            PoolManager.Instance.Despawn(gameObject);

            AudioManager.Instance?.PlayPickup();
            ParticleFX.PickupSparkle(position);
        }
    }
}

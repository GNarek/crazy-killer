using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private BuffDefinition buff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BuffReceiver receiver))
        {
            receiver.ApplyBuff(buff);
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}

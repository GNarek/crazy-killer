using UnityEngine;

public class Targeting : MonoBehaviour
{
    [SerializeField] private float range = 6f;
    [SerializeField] private LayerMask targetLayer;

    public Transform FindClosest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, targetLayer);
        Transform closest = null;
        float closestDist = float.MaxValue;
        foreach (Collider hit in hits)
        {
            float dist = (hit.transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }
        return closest;
    }
}

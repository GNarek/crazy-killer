using UnityEngine;

public class LaneMover : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 direction = Vector3.forward;

    private void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}

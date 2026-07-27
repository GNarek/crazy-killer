using UnityEngine;

public class Bobber : MonoBehaviour
{
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float spinSpeed = 90f;

    private float previousOffset;

    private void OnEnable()
    {
        previousOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
    }

    private void Update()
    {
        float currentOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        float deltaY = currentOffset - previousOffset;
        previousOffset = currentOffset;

        transform.position += new Vector3(0f, deltaY, 0f);
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}

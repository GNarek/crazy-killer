using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterMovement : MonoBehaviour
{
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Pointer.current == null || !Pointer.current.press.isPressed) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(hitPoint.x, minX, maxX);
            transform.position = pos;
        }
    }
}

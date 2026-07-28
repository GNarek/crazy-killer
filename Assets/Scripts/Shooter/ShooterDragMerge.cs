using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterDragMerge : MonoBehaviour
{
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;
    [SerializeField] private float unitY = 0.5f;
    [SerializeField] private float grabRadius = 0.8f;
    [SerializeField] private float mergeRadius = 0.6f;
    [SerializeField] private Camera targetCamera;

    private ShooterUnit draggedUnit;
    private bool wasPressed;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Pointer.current == null || SquadManager.Instance == null) return;
        if (!TryGetWorldX(Pointer.current.position.ReadValue(), out float worldX))
        {
            wasPressed = Pointer.current.press.isPressed;
            return;
        }

        bool isPressed = Pointer.current.press.isPressed;

        if (isPressed && !wasPressed)
        {
            BeginDrag(worldX);
        }
        else if (isPressed && draggedUnit != null)
        {
            ContinueDrag(worldX);
        }
        else if (!isPressed && wasPressed && draggedUnit != null)
        {
            EndDrag(worldX);
        }

        wasPressed = isPressed;
    }

    private void BeginDrag(float worldX)
    {
        draggedUnit = SquadManager.Instance.GetClosestUnit(worldX, grabRadius);
        if (draggedUnit != null)
        {
            SquadManager.Instance.FreeSlot(draggedUnit);
        }
    }

    private void ContinueDrag(float worldX)
    {
        Vector3 pos = draggedUnit.transform.position;
        pos.x = Mathf.Clamp(worldX, minX, maxX);
        draggedUnit.transform.position = pos;
    }

    private void EndDrag(float worldX)
    {
        ShooterUnit target = SquadManager.Instance.GetClosestUnit(worldX, mergeRadius, draggedUnit);

        if (target != null && SquadManager.Instance.TryMerge(draggedUnit, target))
        {
            draggedUnit = null;
            return;
        }

        int slot = SquadManager.Instance.FindNearestEmptySlot(draggedUnit.transform.position.x);
        if (slot >= 0)
        {
            SquadManager.Instance.PlaceInSlot(draggedUnit, slot);
        }

        draggedUnit = null;
    }

    private bool TryGetWorldX(Vector2 screenPos, out float worldX)
    {
        worldX = 0f;
        Ray ray = targetCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, unitY, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            worldX = ray.GetPoint(distance).x;
            return true;
        }

        return false;
    }
}

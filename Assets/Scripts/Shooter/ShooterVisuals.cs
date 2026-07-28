using UnityEngine;

public class ShooterVisuals : MonoBehaviour
{
    private void Awake()
    {
        ShooterManager.ShooterType selected = ShooterManager.Selected;

        if (TryGetComponent(out Renderer renderer))
        {
            renderer.material.color = ShooterManager.GetColor(selected);
        }

        transform.localScale = ShooterManager.GetScale(selected);
    }
}

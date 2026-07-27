using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    private Health health;
    private Color baseColor;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        baseColor = targetRenderer.sharedMaterial.color;
        propertyBlock = new MaterialPropertyBlock();
        health.DamageTaken += HandleDamageTaken;
    }

    private void OnDestroy()
    {
        if (health != null) health.DamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(float amount)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetColor(baseColor);
    }

    private void SetColor(Color color)
    {
        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, color);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}

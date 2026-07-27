using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathPop : MonoBehaviour, IPoolable
{
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popDuration = 0.15f;

    private Health health;
    private Vector3 baseScale;
    private Coroutine popRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        baseScale = transform.localScale;
        health.Died += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null) health.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float factor = Mathf.Lerp(1f, popScale, t / popDuration);
            transform.localScale = baseScale * factor;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    public void OnSpawn() => transform.localScale = baseScale;
    public void OnDespawn() { }
}

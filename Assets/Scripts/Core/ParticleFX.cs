using UnityEngine;

public static class ParticleFX
{
    private static Material cachedMaterial;

    public static void MuzzleFlash(Vector3 position)
    {
        SpawnBurst(position, new Color(1f, 0.95f, 0.6f), size: 0.15f, count: 6, speed: 1.5f, lifetime: 0.12f);
    }

    public static void EnemyDeathBurst(Vector3 position)
    {
        SpawnBurst(position, new Color(1f, 0.4f, 0.15f), size: 0.2f, count: 12, speed: 2.5f, lifetime: 0.3f);
    }

    public static void PickupSparkle(Vector3 position)
    {
        SpawnBurst(position, new Color(1f, 0.9f, 0.3f), size: 0.15f, count: 10, speed: 1.8f, lifetime: 0.4f);
    }

    private static void SpawnBurst(Vector3 position, Color color, float size, int count, float speed, float lifetime)
    {
        GameObject go = new GameObject("ParticleBurst");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = lifetime;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }

    private static Material GetMaterial()
    {
        if (cachedMaterial != null) return cachedMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        cachedMaterial = new Material(shader);
        cachedMaterial.SetFloat("_Smoothness", 0.05f);
        return cachedMaterial;
    }
}

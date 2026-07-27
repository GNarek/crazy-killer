using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneSetup
{
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string DataFolder = "Assets/Data/EnemyDefinitions";
    private const string EnemyPrefabPath = PrefabsFolder + "/Enemy.prefab";
    private const string ProjectilePrefabPath = PrefabsFolder + "/Projectile.prefab";
    private const string BasicEnemyDataPath = DataFolder + "/BasicEnemy.asset";

    [MenuItem("Tools/Crazy Killer/Setup V1 Scene")]
    public static void SetupScene()
    {
        int enemiesLayer = LayerMask.NameToLayer("Enemies");
        if (enemiesLayer == -1)
        {
            EditorUtility.DisplayDialog("Missing Layer",
                "Create a Layer named \"Enemies\" first (Inspector > Layer dropdown > Add Layer), then run this again.",
                "OK");
            return;
        }

        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
        if (projectilePrefab == null)
        {
            EditorUtility.DisplayDialog("Missing Prefab",
                "Couldn't find Assets/Prefabs/Projectile.prefab. Create it first.",
                "OK");
            return;
        }

        GameObject enemyPrefab = CreateEnemyPrefab(enemiesLayer);
        EnemyDefinition enemyDefinition = CreateEnemyDefinition(enemyPrefab);
        Transform spawnPoint = CreateSpawnPoint();
        CreateShooter(projectilePrefab, enemiesLayer);
        CreateWaveSpawner(enemyDefinition, spawnPoint);
        PositionCamera();
        ConfigureLaneMovement();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Setup Complete",
            "Scene is ready. Save the scene (Cmd+S) and press Play to test.",
            "Great");
    }

    private static GameObject CreateEnemyPrefab(int enemiesLayer)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (existing != null) return existing;

        EnsureFolder(PrefabsFolder);

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.layer = enemiesLayer;

        EnemyController controller = enemy.AddComponent<EnemyController>();
        LaneMover mover = enemy.GetComponent<LaneMover>();
        mover.direction = new Vector3(0f, 0f, -1f);
        mover.speed = 2f;

        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("goalZ").floatValue = 0f;
        controllerSO.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static EnemyDefinition CreateEnemyDefinition(GameObject enemyPrefab)
    {
        EnemyDefinition existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BasicEnemyDataPath);
        if (existing != null) return existing;

        EnsureFolder(DataFolder);

        EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.id = "basic_enemy";
        definition.prefab = enemyPrefab;
        definition.maxHealth = 10f;
        definition.moveSpeed = 2f;
        definition.damage = 1f;
        definition.scoreValue = 1;

        AssetDatabase.CreateAsset(definition, BasicEnemyDataPath);
        return definition;
    }

    private static Transform CreateSpawnPoint()
    {
        GameObject existing = GameObject.Find("EnemySpawnPoint");
        if (existing != null) return existing.transform;

        GameObject spawnPoint = new GameObject("EnemySpawnPoint");
        spawnPoint.transform.position = new Vector3(0f, 0.5f, 20f);
        return spawnPoint.transform;
    }

    private static void CreateShooter(GameObject projectilePrefab, int enemiesLayer)
    {
        if (GameObject.Find("Shooter") != null) return;

        GameObject shooter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shooter.name = "Shooter";
        shooter.transform.position = new Vector3(0f, 0.5f, -2f);

        ShooterController shooterController = shooter.AddComponent<ShooterController>();
        Weapon weapon = shooter.GetComponent<Weapon>();
        Targeting targeting = shooter.GetComponent<Targeting>();

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        weaponSO.FindProperty("damage").floatValue = 2f;
        weaponSO.FindProperty("projectileSpeed").floatValue = 12f;
        weaponSO.ApplyModifiedProperties();

        SerializedObject targetingSO = new SerializedObject(targeting);
        targetingSO.FindProperty("range").floatValue = 8f;
        targetingSO.FindProperty("targetLayer").intValue = 1 << enemiesLayer;
        targetingSO.ApplyModifiedProperties();

        SerializedObject controllerSO = new SerializedObject(shooterController);
        controllerSO.FindProperty("baseFireRate").floatValue = 1.5f;
        controllerSO.ApplyModifiedProperties();
    }

    private static void CreateWaveSpawner(EnemyDefinition definition, Transform spawnPoint)
    {
        if (GameObject.Find("WaveSpawner") != null) return;

        GameObject spawnerGO = new GameObject("WaveSpawner");
        WaveSpawner spawner = spawnerGO.AddComponent<WaveSpawner>();

        SerializedObject spawnerSO = new SerializedObject(spawner);
        SerializedProperty enemyPoolProp = spawnerSO.FindProperty("enemyPool");
        enemyPoolProp.ClearArray();
        enemyPoolProp.InsertArrayElementAtIndex(0);
        enemyPoolProp.GetArrayElementAtIndex(0).objectReferenceValue = definition;

        spawnerSO.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
        spawnerSO.FindProperty("spawnInterval").floatValue = 1.5f;
        spawnerSO.ApplyModifiedProperties();
    }

    private const float LaneMinX = -3f;
    private const float LaneMaxX = 3f;

    private static void ConfigureLaneMovement()
    {
        GameObject shooterGO = GameObject.Find("Shooter");
        if (shooterGO != null)
        {
            ShooterMovement movement = shooterGO.GetComponent<ShooterMovement>();
            if (movement == null) movement = shooterGO.AddComponent<ShooterMovement>();

            SerializedObject movementSO = new SerializedObject(movement);
            movementSO.FindProperty("minX").floatValue = LaneMinX;
            movementSO.FindProperty("maxX").floatValue = LaneMaxX;
            movementSO.ApplyModifiedProperties();
        }

        GameObject spawnerGO = GameObject.Find("WaveSpawner");
        if (spawnerGO != null && spawnerGO.TryGetComponent(out WaveSpawner spawner))
        {
            SerializedObject spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("minX").floatValue = LaneMinX;
            spawnerSO.FindProperty("maxX").floatValue = LaneMaxX;
            spawnerSO.ApplyModifiedProperties();
        }
    }

    private static void PositionCamera()
    {
        Camera main = Camera.main;
        if (main == null) return;

        main.transform.position = new Vector3(0f, 14f, -12f);
        main.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}

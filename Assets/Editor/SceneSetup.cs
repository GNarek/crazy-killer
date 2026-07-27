using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SceneSetup
{
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string DataFolder = "Assets/Data/EnemyDefinitions";
    private const string BuffsFolder = "Assets/Data/Buffs";
    private const string MaterialsFolder = "Assets/Art/Materials";

    private const string EnemyPrefabPath = PrefabsFolder + "/Enemy.prefab";
    private const string ProjectilePrefabPath = PrefabsFolder + "/Projectile.prefab";
    private const string BasicEnemyDataPath = DataFolder + "/BasicEnemy.asset";
    private const string FireRateBuffPath = BuffsFolder + "/FireRateBuff.asset";
    private const string DamageBuffPath = BuffsFolder + "/DamageBuff.asset";
    private const string MultiShotBuffPath = BuffsFolder + "/MultiShotBuff.asset";
    private const string FireRatePickupPath = PrefabsFolder + "/PickupFireRate.prefab";
    private const string DamagePickupPath = PrefabsFolder + "/PickupDamage.prefab";
    private const string MultiShotPickupPath = PrefabsFolder + "/PickupMultiShot.prefab";

    private const float LaneMinX = -3f;
    private const float LaneMaxX = 3f;

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
        Transform enemySpawnPoint = CreateSpawnPoint("EnemySpawnPoint", 20f);
        Transform pickupSpawnPoint = CreateSpawnPoint("PickupSpawnPoint", 14f);

        CreateShooter(projectilePrefab);
        CreateWaveSpawner(enemyDefinition, enemySpawnPoint);
        CreateDefenseWall();
        CreateGround();
        PositionCamera();

        ConfigureLaneMovement(pickupSpawnPoint);
        EnsureProjectileCollisionSetup(enemiesLayer);
        EnhanceEnemyPrefab();
        ApplyShooterVisuals();
        ApplyWallVisuals();

        List<GameObject> pickupPrefabs = CreatePickupPrefabs();
        CreatePickupSpawner(pickupPrefabs, pickupSpawnPoint);

        CreateHUD();
        CleanUpObsoleteComponents();

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

        enemy.AddComponent<EnemyController>();
        LaneMover mover = enemy.GetComponent<LaneMover>();
        mover.direction = new Vector3(0f, 0f, -1f);
        mover.speed = 2f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static void EnhanceEnemyPrefab()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);

        if (contents.TryGetComponent(out EnemyController controller))
        {
            SerializedObject controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("goalZ").floatValue = -3f;
            controllerSO.ApplyModifiedProperties();
        }

        if (contents.GetComponent<HitFlash>() == null) contents.AddComponent<HitFlash>();
        if (contents.GetComponent<DeathPop>() == null) contents.AddComponent<DeathPop>();

        if (contents.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial("Enemy", new Color(0.85f, 0.25f, 0.2f));
        }

        PrefabUtility.SaveAsPrefabAsset(contents, EnemyPrefabPath);
        PrefabUtility.UnloadPrefabContents(contents);
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

    private static Transform CreateSpawnPoint(string name, float z)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing.transform;

        GameObject spawnPoint = new GameObject(name);
        spawnPoint.transform.position = new Vector3(0f, 0.5f, z);
        return spawnPoint.transform;
    }

    private static void CreateShooter(GameObject projectilePrefab)
    {
        if (GameObject.Find("Shooter") != null) return;

        GameObject shooter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shooter.name = "Shooter";
        shooter.transform.position = new Vector3(0f, 0.5f, -2f);

        ShooterController shooterController = shooter.AddComponent<ShooterController>();
        Weapon weapon = shooter.GetComponent<Weapon>();

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        weaponSO.FindProperty("damage").floatValue = 2f;
        weaponSO.FindProperty("projectileSpeed").floatValue = 12f;
        weaponSO.ApplyModifiedProperties();

        SerializedObject controllerSO = new SerializedObject(shooterController);
        controllerSO.FindProperty("baseFireRate").floatValue = 1.5f;
        controllerSO.ApplyModifiedProperties();
    }

    private static void ApplyShooterVisuals()
    {
        GameObject shooterGO = GameObject.Find("Shooter");
        if (shooterGO != null && shooterGO.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial("Shooter", new Color(0.2f, 0.5f, 1f));
        }
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

    private static void CreateDefenseWall()
    {
        if (GameObject.Find("DefenseWall") != null) return;

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "DefenseWall";
        wall.transform.position = new Vector3(0f, 0.75f, -3.5f);
        wall.transform.localScale = new Vector3(8f, 1.5f, 0.5f);

        wall.AddComponent<DefenseWall>();
        Health health = wall.GetComponent<Health>();

        SerializedObject healthSO = new SerializedObject(health);
        healthSO.FindProperty("maxHealth").floatValue = 30f;
        healthSO.ApplyModifiedProperties();
    }

    private static void ApplyWallVisuals()
    {
        GameObject wallGO = GameObject.Find("DefenseWall");
        if (wallGO == null) return;

        if (wallGO.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial("Wall", new Color(0.45f, 0.35f, 0.25f));
        }

        if (wallGO.GetComponent<HitFlash>() == null) wallGO.AddComponent<HitFlash>();
    }

    private static void CreateGround()
    {
        if (GameObject.Find("Ground") != null) return;

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, 0f, 8.5f);
        ground.transform.localScale = new Vector3(1f, 1f, 2.7f);

        if (ground.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial("Ground", new Color(0.25f, 0.55f, 0.3f));
        }
    }

    private static void ConfigureLaneMovement(Transform pickupSpawnPoint)
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

            if (shooterGO.GetComponent<BuffReceiver>() == null) shooterGO.AddComponent<BuffReceiver>();
            if (shooterGO.GetComponent<BuffPopupSpawner>() == null) shooterGO.AddComponent<BuffPopupSpawner>();
        }

        GameObject waveSpawnerGO = GameObject.Find("WaveSpawner");
        if (waveSpawnerGO != null && waveSpawnerGO.TryGetComponent(out WaveSpawner spawner))
        {
            SerializedObject spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("minX").floatValue = LaneMinX;
            spawnerSO.FindProperty("maxX").floatValue = LaneMaxX;
            spawnerSO.ApplyModifiedProperties();
        }

        GameObject mainCameraGO = Camera.main != null ? Camera.main.gameObject : null;
        if (mainCameraGO != null && mainCameraGO.GetComponent<CameraShake>() == null)
        {
            mainCameraGO.AddComponent<CameraShake>();
        }
    }

    private static void EnsureProjectileCollisionSetup(int enemiesLayer)
    {
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(ProjectilePrefabPath);

        if (prefabContents.GetComponent<Collider>() == null)
        {
            SphereCollider collider = prefabContents.AddComponent<SphereCollider>();
            collider.isTrigger = true;
        }

        if (prefabContents.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = prefabContents.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (prefabContents.TryGetComponent(out Projectile projectile))
        {
            SerializedObject projectileSO = new SerializedObject(projectile);
            projectileSO.FindProperty("hittableLayers").intValue = 1 << enemiesLayer;
            projectileSO.ApplyModifiedProperties();
        }

        if (prefabContents.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial("Projectile", new Color(1f, 0.9f, 0.2f));
        }

        PrefabUtility.SaveAsPrefabAsset(prefabContents, ProjectilePrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabContents);
    }

    private static List<GameObject> CreatePickupPrefabs()
    {
        BuffDefinition fireRateBuff = CreateBuffDefinition(FireRateBuffPath, "fire_rate_buff", BuffType.FireRate, 1.5f, 0f);
        BuffDefinition damageBuff = CreateBuffDefinition(DamageBuffPath, "damage_buff", BuffType.Damage, 2.5f, 0f);
        BuffDefinition multiShotBuff = CreateBuffDefinition(MultiShotBuffPath, "multi_shot_buff", BuffType.MultiShot, 1f, 0f);

        GameObject fireRatePickup = CreatePickupPrefab(FireRatePickupPath, "PickupFireRate", fireRateBuff, new Color(1f, 0.6f, 0.1f));
        GameObject damagePickup = CreatePickupPrefab(DamagePickupPath, "PickupDamage", damageBuff, new Color(0.8f, 0.2f, 0.7f));
        GameObject multiShotPickup = CreatePickupPrefab(MultiShotPickupPath, "PickupMultiShot", multiShotBuff, new Color(0.1f, 0.85f, 0.85f));

        return new List<GameObject> { fireRatePickup, damagePickup, multiShotPickup };
    }

    private static BuffDefinition CreateBuffDefinition(string path, string id, BuffType type, float value, float duration)
    {
        BuffDefinition existing = AssetDatabase.LoadAssetAtPath<BuffDefinition>(path);
        if (existing != null) return existing;

        EnsureFolder(BuffsFolder);

        BuffDefinition definition = ScriptableObject.CreateInstance<BuffDefinition>();
        definition.id = id;
        definition.type = type;
        definition.value = value;
        definition.duration = duration;

        AssetDatabase.CreateAsset(definition, path);
        return definition;
    }

    private static GameObject CreatePickupPrefab(string path, string materialName, BuffDefinition buff, Color color)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        EnsureFolder(PrefabsFolder);

        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = materialName;
        pickup.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        Object.DestroyImmediate(pickup.GetComponent<Collider>());
        SphereCollider collider = pickup.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        LaneMover mover = pickup.AddComponent<LaneMover>();
        mover.direction = new Vector3(0f, 0f, -1f);
        mover.speed = 2.5f;

        pickup.AddComponent<AutoDespawn>();
        pickup.AddComponent<Bobber>();

        PickupItem pickupItem = pickup.AddComponent<PickupItem>();
        SerializedObject pickupSO = new SerializedObject(pickupItem);
        pickupSO.FindProperty("buff").objectReferenceValue = buff;
        pickupSO.ApplyModifiedProperties();

        if (pickup.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = GetOrCreateMaterial(materialName, color);
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pickup, path);
        Object.DestroyImmediate(pickup);
        return prefab;
    }

    private static void CreatePickupSpawner(List<GameObject> pickupPrefabs, Transform spawnPoint)
    {
        GameObject spawnerGO = GameObject.Find("PickupSpawner");
        PickupSpawner spawner;
        if (spawnerGO == null)
        {
            spawnerGO = new GameObject("PickupSpawner");
            spawner = spawnerGO.AddComponent<PickupSpawner>();
        }
        else
        {
            spawner = spawnerGO.GetComponent<PickupSpawner>();
        }

        SerializedObject spawnerSO = new SerializedObject(spawner);
        SerializedProperty poolProp = spawnerSO.FindProperty("pickupPrefabs");
        poolProp.ClearArray();
        for (int i = 0; i < pickupPrefabs.Count; i++)
        {
            poolProp.InsertArrayElementAtIndex(i);
            poolProp.GetArrayElementAtIndex(i).objectReferenceValue = pickupPrefabs[i];
        }

        spawnerSO.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
        spawnerSO.FindProperty("spawnInterval").floatValue = 6f;
        spawnerSO.FindProperty("minX").floatValue = LaneMinX;
        spawnerSO.FindProperty("maxX").floatValue = LaneMaxX;
        spawnerSO.ApplyModifiedProperties();
    }

    private static void PositionCamera()
    {
        Camera main = Camera.main;
        if (main == null) return;

        main.transform.position = new Vector3(0f, 14f, -12f);
        main.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }

    private static void CreateHUD()
    {
        if (GameObject.Find("HUD") != null) return;

        GameObject canvasGO = new GameObject("HUD", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject scoreGO = CreateUIText("ScoreText", canvasGO.transform, defaultFont, "Score: 0", 48, TextAnchor.UpperLeft);
        RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0f, 1f);
        scoreRT.anchorMax = new Vector2(0f, 1f);
        scoreRT.pivot = new Vector2(0f, 1f);
        scoreRT.anchoredPosition = new Vector2(40f, -40f);
        scoreRT.sizeDelta = new Vector2(400f, 80f);

        GameObject barBgGO = new GameObject("WallHealthBarBg", typeof(RectTransform), typeof(Image));
        barBgGO.transform.SetParent(canvasGO.transform, false);
        barBgGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform barBgRT = barBgGO.GetComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0.5f, 1f);
        barBgRT.anchorMax = new Vector2(0.5f, 1f);
        barBgRT.pivot = new Vector2(0.5f, 1f);
        barBgRT.anchoredPosition = new Vector2(0f, -40f);
        barBgRT.sizeDelta = new Vector2(600f, 50f);

        GameObject barFillGO = new GameObject("WallHealthBarFill", typeof(RectTransform), typeof(Image));
        barFillGO.transform.SetParent(barBgGO.transform, false);
        Image barFill = barFillGO.GetComponent<Image>();
        barFill.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.fillAmount = 1f;
        RectTransform barFillRT = barFillGO.GetComponent<RectTransform>();
        barFillRT.anchorMin = Vector2.zero;
        barFillRT.anchorMax = Vector2.one;
        barFillRT.offsetMin = Vector2.zero;
        barFillRT.offsetMax = Vector2.zero;

        GameObject panelGO = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject gameOverTextGO = CreateUIText("GameOverText", panelGO.transform, defaultFont, "GAME OVER", 96, TextAnchor.MiddleCenter);
        RectTransform gameOverTextRT = gameOverTextGO.GetComponent<RectTransform>();
        gameOverTextRT.anchorMin = new Vector2(0.5f, 0.6f);
        gameOverTextRT.anchorMax = new Vector2(0.5f, 0.6f);
        gameOverTextRT.pivot = new Vector2(0.5f, 0.5f);
        gameOverTextRT.anchoredPosition = Vector2.zero;
        gameOverTextRT.sizeDelta = new Vector2(800f, 150f);

        GameObject buttonGO = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(panelGO.transform, false);
        buttonGO.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f, 1f);
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRT.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRT.pivot = new Vector2(0.5f, 0.5f);
        buttonRT.anchoredPosition = Vector2.zero;
        buttonRT.sizeDelta = new Vector2(400f, 120f);

        GameObject buttonTextGO = CreateUIText("Text", buttonGO.transform, defaultFont, "RESTART", 48, TextAnchor.MiddleCenter);
        RectTransform buttonTextRT = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRT.anchorMin = Vector2.zero;
        buttonTextRT.anchorMax = Vector2.one;
        buttonTextRT.offsetMin = Vector2.zero;
        buttonTextRT.offsetMax = Vector2.zero;

        RestartController restartController = panelGO.AddComponent<RestartController>();
        UnityEventTools.AddPersistentListener(buttonGO.GetComponent<Button>().onClick, restartController.Restart);

        panelGO.SetActive(false);

        HUDController hud = canvasGO.AddComponent<HUDController>();
        SerializedObject hudSO = new SerializedObject(hud);
        hudSO.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<Text>();
        hudSO.FindProperty("wallHealthFill").objectReferenceValue = barFill;
        hudSO.FindProperty("gameOverPanel").objectReferenceValue = panelGO;
        hudSO.ApplyModifiedProperties();
    }

    private static GameObject CreateUIText(string name, Transform parent, Font font, string text, int fontSize, TextAnchor anchor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text textComponent = go.GetComponent<Text>();
        textComponent.font = font;
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = anchor;
        textComponent.color = Color.white;
        return go;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<InputSystemUIInputModule>();
    }

    private static void CleanUpObsoleteComponents()
    {
        GameObject shooterGO = GameObject.Find("Shooter");
        if (shooterGO != null)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(shooterGO);
        }
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            ApplyFlatShading(existing);
            return existing;
        }

        EnsureFolder(MaterialsFolder);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader) { color = color };
        ApplyFlatShading(material);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void ApplyFlatShading(Material material)
    {
        material.SetFloat("_Smoothness", 0.05f);
        material.SetFloat("_Metallic", 0f);
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

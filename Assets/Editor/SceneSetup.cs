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
        string activeSceneName = EditorSceneManager.GetActiveScene().name;
        if (activeSceneName != "SampleScene")
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                $"This must be run with \"SampleScene\" open (it's currently \"{activeSceneName}\"). " +
                "Open SampleScene first, then run this again.",
                "OK");
            return;
        }

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
        Transform enemySpawnPoint = CreateSpawnPoint("EnemySpawnPoint", 26f);
        Transform pickupSpawnPoint = CreateSpawnPoint("PickupSpawnPoint", 18f);

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
        CreatePauseUI();
        CreateAudioManager();
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
        GameObject spawnPoint = GameObject.Find(name);
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject(name);
        }

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
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }

        ground.transform.position = new Vector3(0f, 0f, 5f);
        ground.transform.localScale = new Vector3(2.5f, 1f, 12f);

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
        main.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
        main.fieldOfView = 52f;
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

    private static void CreatePauseUI()
    {
        GameObject canvasGO = GameObject.Find("HUD");
        if (canvasGO == null) return;

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject panelGO = GameObject.Find("GameOverPanel");
        if (panelGO != null && panelGO.transform.Find("MainMenuButton") == null)
        {
            if (!panelGO.TryGetComponent(out SceneNavigator gameOverNavigator))
            {
                gameOverNavigator = panelGO.AddComponent<SceneNavigator>();
            }

            Button gameOverMenuButton = CreateButton("MainMenuButton", panelGO.transform, defaultFont, "MAIN MENU",
                new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -80f), new Vector2(400f, 100f));
            UnityEventTools.AddPersistentListener(gameOverMenuButton.onClick, gameOverNavigator.GoToMainMenu);
        }

        if (GameObject.Find("PauseButton") != null) return;

        GameObject pauseButtonGO = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        pauseButtonGO.transform.SetParent(canvasGO.transform, false);
        pauseButtonGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform pauseButtonRT = pauseButtonGO.GetComponent<RectTransform>();
        pauseButtonRT.anchorMin = new Vector2(1f, 1f);
        pauseButtonRT.anchorMax = new Vector2(1f, 1f);
        pauseButtonRT.pivot = new Vector2(1f, 1f);
        pauseButtonRT.anchoredPosition = new Vector2(-30f, -30f);
        pauseButtonRT.sizeDelta = new Vector2(100f, 100f);
        GameObject pauseButtonTextGO = CreateUIText("Text", pauseButtonGO.transform, defaultFont, "II", 44, TextAnchor.MiddleCenter);
        RectTransform pauseButtonTextRT = pauseButtonTextGO.GetComponent<RectTransform>();
        pauseButtonTextRT.anchorMin = Vector2.zero;
        pauseButtonTextRT.anchorMax = Vector2.one;
        pauseButtonTextRT.offsetMin = Vector2.zero;
        pauseButtonTextRT.offsetMax = Vector2.zero;

        GameObject pausePanelGO = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        pausePanelGO.transform.SetParent(canvasGO.transform, false);
        pausePanelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform pausePanelRT = pausePanelGO.GetComponent<RectTransform>();
        pausePanelRT.anchorMin = Vector2.zero;
        pausePanelRT.anchorMax = Vector2.one;
        pausePanelRT.offsetMin = Vector2.zero;
        pausePanelRT.offsetMax = Vector2.zero;

        GameObject pausedTextGO = CreateUIText("PausedText", pausePanelGO.transform, defaultFont, "PAUSED", 96, TextAnchor.MiddleCenter);
        RectTransform pausedTextRT = pausedTextGO.GetComponent<RectTransform>();
        pausedTextRT.anchorMin = new Vector2(0.5f, 0.7f);
        pausedTextRT.anchorMax = new Vector2(0.5f, 0.7f);
        pausedTextRT.pivot = new Vector2(0.5f, 0.5f);
        pausedTextRT.anchoredPosition = Vector2.zero;
        pausedTextRT.sizeDelta = new Vector2(800f, 150f);

        PauseController pauseController = pausePanelGO.AddComponent<PauseController>();

        Button resumeButton = CreateButton("ResumeButton", pausePanelGO.transform, defaultFont, "RESUME",
            new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(0f, 60f), new Vector2(400f, 110f));
        UnityEventTools.AddPersistentListener(resumeButton.onClick, pauseController.Resume);

        Button pauseRestartButton = CreateButton("RestartButton", pausePanelGO.transform, defaultFont, "RESTART",
            new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(0f, -70f), new Vector2(400f, 110f));
        RestartController pauseRestartController = pausePanelGO.AddComponent<RestartController>();
        UnityEventTools.AddPersistentListener(pauseRestartButton.onClick, pauseRestartController.Restart);

        Button pauseMenuButton = CreateButton("MainMenuButton", pausePanelGO.transform, defaultFont, "MAIN MENU",
            new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -200f), new Vector2(400f, 100f));
        SceneNavigator pauseNavigator = pausePanelGO.AddComponent<SceneNavigator>();
        UnityEventTools.AddPersistentListener(pauseMenuButton.onClick, pauseNavigator.GoToMainMenu);

        CreateMuteToggle("MuteToggle", pausePanelGO.transform, defaultFont, new Vector2(0f, -320f));

        SerializedObject pauseControllerSO = new SerializedObject(pauseController);
        pauseControllerSO.FindProperty("pausePanel").objectReferenceValue = pausePanelGO;
        pauseControllerSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(pauseButtonGO.GetComponent<Button>().onClick, pauseController.TogglePause);

        pausePanelGO.SetActive(false);
    }

    internal static Button CreateButton(string name, Transform parent, Font font, string label, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);
        buttonGO.GetComponent<Image>().color = color;
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRT.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRT.pivot = new Vector2(0.5f, 0.5f);
        buttonRT.anchoredPosition = anchoredPosition;
        buttonRT.sizeDelta = size;

        GameObject textGO = CreateUIText("Text", buttonGO.transform, font, label, 40, TextAnchor.MiddleCenter);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return buttonGO.GetComponent<Button>();
    }

    internal static void CreateMuteToggle(string name, Transform parent, Font font, Vector2 anchoredPosition)
    {
        GameObject toggleGO = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        toggleGO.transform.SetParent(parent, false);
        RectTransform toggleRT = toggleGO.GetComponent<RectTransform>();
        toggleRT.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRT.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRT.pivot = new Vector2(0.5f, 0.5f);
        toggleRT.anchoredPosition = anchoredPosition;
        toggleRT.sizeDelta = new Vector2(60f, 60f);

        GameObject backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundGO.transform.SetParent(toggleGO.transform, false);
        Image background = backgroundGO.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.3f);
        RectTransform backgroundRT = backgroundGO.GetComponent<RectTransform>();
        backgroundRT.anchorMin = Vector2.zero;
        backgroundRT.anchorMax = Vector2.one;
        backgroundRT.offsetMin = Vector2.zero;
        backgroundRT.offsetMax = Vector2.zero;

        GameObject checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(backgroundGO.transform, false);
        Image checkmark = checkGO.GetComponent<Image>();
        checkmark.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        RectTransform checkRT = checkGO.GetComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0.2f, 0.2f);
        checkRT.anchorMax = new Vector2(0.8f, 0.8f);
        checkRT.offsetMin = Vector2.zero;
        checkRT.offsetMax = Vector2.zero;

        Toggle toggle = toggleGO.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;

        GameObject labelGO = CreateUIText("Label", toggleGO.transform, font, "MUTE", 32, TextAnchor.MiddleLeft);
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(1f, 0f);
        labelRT.anchorMax = new Vector2(1f, 1f);
        labelRT.pivot = new Vector2(0f, 0.5f);
        labelRT.anchoredPosition = new Vector2(20f, 0f);
        labelRT.sizeDelta = new Vector2(200f, 0f);

        toggleGO.AddComponent<MuteToggle>();
    }

    internal static GameObject CreateUIText(string name, Transform parent, Font font, string text, int fontSize, TextAnchor anchor)
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

    internal static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<InputSystemUIInputModule>();
    }

    private static void CreateAudioManager()
    {
        if (GameObject.Find("AudioManager") != null) return;

        GameObject audioManager = new GameObject("AudioManager");
        audioManager.AddComponent<AudioManager>();
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

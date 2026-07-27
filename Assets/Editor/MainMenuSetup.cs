using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSetup
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Crazy Killer/Setup Main Menu Scene")]
    public static void SetupMainMenu()
    {
        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        EnsureCamera();
        EnsureCanvas();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddScenesToBuildSettings();

        EditorUtility.DisplayDialog("Main Menu Ready",
            "MainMenu scene created and set as build index 0 (SampleScene is index 1).\n\nPress Play here to test, or reopen SampleScene to keep working on gameplay.",
            "OK");
    }

    private static void EnsureCamera()
    {
        if (Camera.main != null) return;

        GameObject cameraGO = new GameObject("Main Camera");
        Camera camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.15f, 0.45f, 0.25f);
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<AudioListener>();
    }

    private static void EnsureCanvas()
    {
        GameObject canvasGO = GameObject.Find("MainMenuCanvas");
        bool isNewCanvas = canvasGO == null;

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (isNewCanvas)
        {
            canvasGO = new GameObject("MainMenuCanvas", typeof(RectTransform));
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            canvasGO.AddComponent<GraphicRaycaster>();

            SceneSetup.EnsureEventSystem();

            GameObject titleGO = SceneSetup.CreateUIText("Title", canvasGO.transform, defaultFont, "CRAZY KILLER", 90, TextAnchor.MiddleCenter);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.65f);
            titleRT.anchorMax = new Vector2(0.5f, 0.65f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = Vector2.zero;
            titleRT.sizeDelta = new Vector2(900f, 300f);

            Button playButton = SceneSetup.CreateButton("PlayButton", canvasGO.transform, defaultFont, "PLAY",
                new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(0f, 60f), new Vector2(400f, 140f));

            MainMenuController menuController = canvasGO.AddComponent<MainMenuController>();
            UnityEventTools.AddPersistentListener(playButton.onClick, menuController.PlayGame);

            SceneSetup.CreateMuteToggle("MuteToggle", canvasGO.transform, defaultFont, new Vector2(0f, -240f));
        }

        if (canvasGO.transform.Find("UpgradesButton") == null)
        {
            Button upgradesButton = SceneSetup.CreateButton("UpgradesButton", canvasGO.transform, defaultFont, "UPGRADES",
                new Color(0.8f, 0.6f, 0.2f, 1f), new Vector2(0f, -110f), new Vector2(400f, 110f));

            GameObject upgradePanel = CreateUpgradePanel(canvasGO.transform, defaultFont);

            if (!canvasGO.TryGetComponent(out PanelToggle panelToggle))
            {
                panelToggle = canvasGO.AddComponent<PanelToggle>();
            }
            SerializedObject panelToggleSO = new SerializedObject(panelToggle);
            panelToggleSO.FindProperty("panel").objectReferenceValue = upgradePanel;
            panelToggleSO.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(upgradesButton.onClick, panelToggle.Open);
        }
    }

    private static GameObject CreateUpgradePanel(Transform parent, Font font)
    {
        GameObject panelGO = new GameObject("UpgradePanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(parent, false);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject titleGO = SceneSetup.CreateUIText("Title", panelGO.transform, font, "UPGRADES", 72, TextAnchor.MiddleCenter);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = new Vector2(0f, 650f);
        titleRT.sizeDelta = new Vector2(800f, 150f);

        GameObject coinsGO = SceneSetup.CreateUIText("CoinsText", panelGO.transform, font, "Coins: 0", 48, TextAnchor.MiddleCenter);
        RectTransform coinsRT = coinsGO.GetComponent<RectTransform>();
        coinsRT.anchorMin = new Vector2(0.5f, 0.5f);
        coinsRT.anchorMax = new Vector2(0.5f, 0.5f);
        coinsRT.pivot = new Vector2(0.5f, 0.5f);
        coinsRT.anchoredPosition = new Vector2(0f, 520f);
        coinsRT.sizeDelta = new Vector2(600f, 80f);
        coinsGO.GetComponent<Text>().color = new Color(1f, 0.85f, 0.2f);

        CreateUpgradeRow(panelGO.transform, font, "DAMAGE", 260f, out Text damageLevel, out Text damageCost, out Button damageBuy);
        CreateUpgradeRow(panelGO.transform, font, "FIRE RATE", 80f, out Text fireRateLevel, out Text fireRateCost, out Button fireRateBuy);
        CreateUpgradeRow(panelGO.transform, font, "WALL HP", -100f, out Text wallLevel, out Text wallCost, out Button wallBuy);

        Button closeButton = SceneSetup.CreateButton("CloseButton", panelGO.transform, font, "CLOSE",
            new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -300f), new Vector2(400f, 110f));

        UpgradeShopController shop = panelGO.AddComponent<UpgradeShopController>();
        SerializedObject shopSO = new SerializedObject(shop);
        shopSO.FindProperty("coinsText").objectReferenceValue = coinsGO.GetComponent<Text>();
        shopSO.FindProperty("damageLevelText").objectReferenceValue = damageLevel;
        shopSO.FindProperty("damageCostText").objectReferenceValue = damageCost;
        shopSO.FindProperty("damageBuyButton").objectReferenceValue = damageBuy;
        shopSO.FindProperty("fireRateLevelText").objectReferenceValue = fireRateLevel;
        shopSO.FindProperty("fireRateCostText").objectReferenceValue = fireRateCost;
        shopSO.FindProperty("fireRateBuyButton").objectReferenceValue = fireRateBuy;
        shopSO.FindProperty("wallHealthLevelText").objectReferenceValue = wallLevel;
        shopSO.FindProperty("wallHealthCostText").objectReferenceValue = wallCost;
        shopSO.FindProperty("wallHealthBuyButton").objectReferenceValue = wallBuy;
        shopSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(damageBuy.onClick, shop.BuyDamage);
        UnityEventTools.AddPersistentListener(fireRateBuy.onClick, shop.BuyFireRate);
        UnityEventTools.AddPersistentListener(wallBuy.onClick, shop.BuyWallHealth);

        PanelToggle closeToggle = panelGO.AddComponent<PanelToggle>();
        SerializedObject closeToggleSO = new SerializedObject(closeToggle);
        closeToggleSO.FindProperty("panel").objectReferenceValue = panelGO;
        closeToggleSO.ApplyModifiedProperties();
        UnityEventTools.AddPersistentListener(closeButton.onClick, closeToggle.Close);

        panelGO.SetActive(false);
        return panelGO;
    }

    private static void CreateUpgradeRow(Transform parent, Font font, string label, float yPos,
        out Text levelText, out Text costText, out Button buyButton)
    {
        GameObject labelGO = SceneSetup.CreateUIText(label + "Label", parent, font, label, 36, TextAnchor.MiddleLeft);
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0.5f, 0.5f);
        labelRT.anchorMax = new Vector2(0.5f, 0.5f);
        labelRT.pivot = new Vector2(0.5f, 0.5f);
        labelRT.anchoredPosition = new Vector2(-330f, yPos);
        labelRT.sizeDelta = new Vector2(320f, 100f);

        GameObject levelGO = SceneSetup.CreateUIText(label + "Level", parent, font, "Lv 0", 32, TextAnchor.MiddleCenter);
        RectTransform levelRT = levelGO.GetComponent<RectTransform>();
        levelRT.anchorMin = new Vector2(0.5f, 0.5f);
        levelRT.anchorMax = new Vector2(0.5f, 0.5f);
        levelRT.pivot = new Vector2(0.5f, 0.5f);
        levelRT.anchoredPosition = new Vector2(-50f, yPos);
        levelRT.sizeDelta = new Vector2(150f, 100f);
        levelText = levelGO.GetComponent<Text>();

        Button button = SceneSetup.CreateButton(label + "BuyButton", parent, font, "0",
            new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(230f, yPos), new Vector2(280f, 100f));
        buyButton = button;
        costText = button.GetComponentInChildren<Text>();
    }

    private static void AddScenesToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene(GameplayScenePath, true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

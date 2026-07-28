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

        if (canvasGO.transform.Find("ShootersButton") == null)
        {
            Button shootersButton = SceneSetup.CreateButton("ShootersButton", canvasGO.transform, defaultFont, "SHOOTERS",
                new Color(0.5f, 0.2f, 0.7f, 1f), new Vector2(0f, -350f), new Vector2(400f, 110f));

            GameObject shooterPanel = CreateShooterPanel(canvasGO.transform, defaultFont);

            GameObject shooterToggleGO = new GameObject("ShooterPanelToggle", typeof(RectTransform));
            shooterToggleGO.transform.SetParent(canvasGO.transform, false);
            PanelToggle shooterPanelToggle = shooterToggleGO.AddComponent<PanelToggle>();
            SerializedObject shooterToggleSO = new SerializedObject(shooterPanelToggle);
            shooterToggleSO.FindProperty("panel").objectReferenceValue = shooterPanel;
            shooterToggleSO.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(shootersButton.onClick, shooterPanelToggle.Open);
        }

        if (canvasGO.transform.Find("DailyRewardButton") == null)
        {
            Button dailyRewardButton = SceneSetup.CreateButton("DailyRewardButton", canvasGO.transform, defaultFont, "DAILY REWARD",
                new Color(0.9f, 0.7f, 0.1f, 1f), new Vector2(0f, -480f), new Vector2(400f, 110f));

            GameObject dailyRewardPanel = CreateDailyRewardPanel(canvasGO.transform, defaultFont);

            GameObject dailyRewardToggleGO = new GameObject("DailyRewardPanelToggle", typeof(RectTransform));
            dailyRewardToggleGO.transform.SetParent(canvasGO.transform, false);
            PanelToggle dailyRewardPanelToggle = dailyRewardToggleGO.AddComponent<PanelToggle>();
            SerializedObject dailyRewardToggleSO = new SerializedObject(dailyRewardPanelToggle);
            dailyRewardToggleSO.FindProperty("panel").objectReferenceValue = dailyRewardPanel;
            dailyRewardToggleSO.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(dailyRewardButton.onClick, dailyRewardPanelToggle.Open);
        }

        if (canvasGO.transform.Find("HighScoresButton") == null)
        {
            Button highScoresButton = SceneSetup.CreateButton("HighScoresButton", canvasGO.transform, defaultFont, "HIGH SCORES",
                new Color(0.2f, 0.7f, 0.5f, 1f), new Vector2(0f, -610f), new Vector2(400f, 110f));

            GameObject highScorePanel = CreateHighScorePanel(canvasGO.transform, defaultFont);

            GameObject highScoreToggleGO = new GameObject("HighScorePanelToggle", typeof(RectTransform));
            highScoreToggleGO.transform.SetParent(canvasGO.transform, false);
            PanelToggle highScorePanelToggle = highScoreToggleGO.AddComponent<PanelToggle>();
            SerializedObject highScoreToggleSO = new SerializedObject(highScorePanelToggle);
            highScoreToggleSO.FindProperty("panel").objectReferenceValue = highScorePanel;
            highScoreToggleSO.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(highScoresButton.onClick, highScorePanelToggle.Open);
        }
    }

    private static GameObject CreateShooterPanel(Transform parent, Font font)
    {
        GameObject panelGO = new GameObject("ShooterPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(parent, false);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject titleGO = SceneSetup.CreateUIText("Title", panelGO.transform, font, "SHOOTERS", 72, TextAnchor.MiddleCenter);
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

        CreateShooterRow(panelGO.transform, font, "STANDARD", 260f, out Text standardStatus, out Button standardAction, out Text standardLabel);
        CreateShooterRow(panelGO.transform, font, "RAPID", 80f, out Text rapidStatus, out Button rapidAction, out Text rapidLabel);
        CreateShooterRow(panelGO.transform, font, "HEAVY", -100f, out Text heavyStatus, out Button heavyAction, out Text heavyLabel);

        Button closeButton = SceneSetup.CreateButton("CloseButton", panelGO.transform, font, "CLOSE",
            new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -300f), new Vector2(400f, 110f));

        ShooterSelectController select = panelGO.AddComponent<ShooterSelectController>();
        SerializedObject selectSO = new SerializedObject(select);
        selectSO.FindProperty("coinsText").objectReferenceValue = coinsGO.GetComponent<Text>();
        selectSO.FindProperty("standardStatusText").objectReferenceValue = standardStatus;
        selectSO.FindProperty("standardActionButton").objectReferenceValue = standardAction;
        selectSO.FindProperty("standardActionLabel").objectReferenceValue = standardLabel;
        selectSO.FindProperty("rapidStatusText").objectReferenceValue = rapidStatus;
        selectSO.FindProperty("rapidActionButton").objectReferenceValue = rapidAction;
        selectSO.FindProperty("rapidActionLabel").objectReferenceValue = rapidLabel;
        selectSO.FindProperty("heavyStatusText").objectReferenceValue = heavyStatus;
        selectSO.FindProperty("heavyActionButton").objectReferenceValue = heavyAction;
        selectSO.FindProperty("heavyActionLabel").objectReferenceValue = heavyLabel;
        selectSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(standardAction.onClick, select.ActionStandard);
        UnityEventTools.AddPersistentListener(rapidAction.onClick, select.ActionRapid);
        UnityEventTools.AddPersistentListener(heavyAction.onClick, select.ActionHeavy);

        PanelToggle closeToggle = panelGO.AddComponent<PanelToggle>();
        SerializedObject closeToggleSO = new SerializedObject(closeToggle);
        closeToggleSO.FindProperty("panel").objectReferenceValue = panelGO;
        closeToggleSO.ApplyModifiedProperties();
        UnityEventTools.AddPersistentListener(closeButton.onClick, closeToggle.Close);

        panelGO.SetActive(false);
        return panelGO;
    }

    private static void CreateShooterRow(Transform parent, Font font, string label, float yPos,
        out Text statusText, out Button actionButton, out Text actionLabel)
    {
        GameObject labelGO = SceneSetup.CreateUIText(label + "Label", parent, font, label, 34, TextAnchor.MiddleLeft);
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0.5f, 0.5f);
        labelRT.anchorMax = new Vector2(0.5f, 0.5f);
        labelRT.pivot = new Vector2(0.5f, 0.5f);
        labelRT.anchoredPosition = new Vector2(-330f, yPos);
        labelRT.sizeDelta = new Vector2(280f, 100f);

        GameObject statusGO = SceneSetup.CreateUIText(label + "Status", parent, font, "", 26, TextAnchor.MiddleCenter);
        RectTransform statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0.5f, 0.5f);
        statusRT.anchorMax = new Vector2(0.5f, 0.5f);
        statusRT.pivot = new Vector2(0.5f, 0.5f);
        statusRT.anchoredPosition = new Vector2(-60f, yPos);
        statusRT.sizeDelta = new Vector2(180f, 100f);
        statusText = statusGO.GetComponent<Text>();

        Button button = SceneSetup.CreateButton(label + "ActionButton", parent, font, "",
            new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(220f, yPos), new Vector2(260f, 100f));
        actionButton = button;
        actionLabel = button.GetComponentInChildren<Text>();
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

    private static GameObject CreateDailyRewardPanel(Transform parent, Font font)
    {
        GameObject panelGO = new GameObject("DailyRewardPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(parent, false);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject titleGO = SceneSetup.CreateUIText("Title", panelGO.transform, font, "DAILY REWARDS", 72, TextAnchor.MiddleCenter);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = new Vector2(0f, 650f);
        titleRT.sizeDelta = new Vector2(900f, 150f);

        Text[] dayTexts = new Text[DailyRewardManager.DayCount];
        float[] row1X = { -330f, -110f, 110f, 330f };
        float[] row2X = { -220f, 0f, 220f };

        for (int i = 0; i < row1X.Length; i++)
        {
            dayTexts[i] = CreateDailySlot(panelGO.transform, font, i + 1, row1X[i], 400f);
        }
        for (int i = 0; i < row2X.Length; i++)
        {
            dayTexts[row1X.Length + i] = CreateDailySlot(panelGO.transform, font, row1X.Length + i + 1, row2X[i], 180f);
        }

        GameObject statusGO = SceneSetup.CreateUIText("StatusText", panelGO.transform, font, "", 34, TextAnchor.MiddleCenter);
        RectTransform statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0.5f, 0.5f);
        statusRT.anchorMax = new Vector2(0.5f, 0.5f);
        statusRT.pivot = new Vector2(0.5f, 0.5f);
        statusRT.anchoredPosition = new Vector2(0f, -20f);
        statusRT.sizeDelta = new Vector2(900f, 100f);
        statusGO.GetComponent<Text>().color = new Color(1f, 0.85f, 0.2f);

        Button claimButton = SceneSetup.CreateButton("ClaimButton", panelGO.transform, font, "CLAIM",
            new Color(0.9f, 0.7f, 0.1f, 1f), new Vector2(0f, -180f), new Vector2(500f, 130f));

        Button closeButton = SceneSetup.CreateButton("CloseButton", panelGO.transform, font, "CLOSE",
            new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -340f), new Vector2(400f, 110f));

        DailyRewardController controller = panelGO.AddComponent<DailyRewardController>();
        SerializedObject controllerSO = new SerializedObject(controller);
        SerializedProperty dayTextsProp = controllerSO.FindProperty("dayTexts");
        dayTextsProp.arraySize = dayTexts.Length;
        for (int i = 0; i < dayTexts.Length; i++)
        {
            dayTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = dayTexts[i];
        }
        controllerSO.FindProperty("claimButton").objectReferenceValue = claimButton;
        controllerSO.FindProperty("claimButtonLabel").objectReferenceValue = claimButton.GetComponentInChildren<Text>();
        controllerSO.FindProperty("statusText").objectReferenceValue = statusGO.GetComponent<Text>();
        controllerSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(claimButton.onClick, controller.Claim);

        PanelToggle closeToggle = panelGO.AddComponent<PanelToggle>();
        SerializedObject closeToggleSO = new SerializedObject(closeToggle);
        closeToggleSO.FindProperty("panel").objectReferenceValue = panelGO;
        closeToggleSO.ApplyModifiedProperties();
        UnityEventTools.AddPersistentListener(closeButton.onClick, closeToggle.Close);

        panelGO.SetActive(false);
        return panelGO;
    }

    private static Text CreateDailySlot(Transform parent, Font font, int day, float xPos, float yPos)
    {
        GameObject slotGO = SceneSetup.CreateUIText($"Day{day}Slot", parent, font, $"Day {day}\n+0", 28, TextAnchor.MiddleCenter);
        RectTransform slotRT = slotGO.GetComponent<RectTransform>();
        slotRT.anchorMin = new Vector2(0.5f, 0.5f);
        slotRT.anchorMax = new Vector2(0.5f, 0.5f);
        slotRT.pivot = new Vector2(0.5f, 0.5f);
        slotRT.anchoredPosition = new Vector2(xPos, yPos);
        slotRT.sizeDelta = new Vector2(200f, 140f);
        return slotGO.GetComponent<Text>();
    }

    private static GameObject CreateHighScorePanel(Transform parent, Font font)
    {
        GameObject panelGO = new GameObject("HighScorePanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(parent, false);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject titleGO = SceneSetup.CreateUIText("Title", panelGO.transform, font, "HIGH SCORES", 72, TextAnchor.MiddleCenter);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = new Vector2(0f, 650f);
        titleRT.sizeDelta = new Vector2(900f, 150f);

        const int rowCount = 10;
        Text[] rankTexts = new Text[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            float yPos = 480f - i * 80f;
            GameObject rowGO = SceneSetup.CreateUIText($"Rank{i + 1}", panelGO.transform, font, $"{i + 1}. —", 36, TextAnchor.MiddleCenter);
            RectTransform rowRT = rowGO.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 0.5f);
            rowRT.anchorMax = new Vector2(0.5f, 0.5f);
            rowRT.pivot = new Vector2(0.5f, 0.5f);
            rowRT.anchoredPosition = new Vector2(0f, yPos);
            rowRT.sizeDelta = new Vector2(600f, 70f);
            rankTexts[i] = rowGO.GetComponent<Text>();
        }

        Button closeButton = SceneSetup.CreateButton("CloseButton", panelGO.transform, font, "CLOSE",
            new Color(0.5f, 0.5f, 0.5f, 1f), new Vector2(0f, -380f), new Vector2(400f, 110f));

        HighScoreController controller = panelGO.AddComponent<HighScoreController>();
        SerializedObject controllerSO = new SerializedObject(controller);
        SerializedProperty rankTextsProp = controllerSO.FindProperty("rankTexts");
        rankTextsProp.arraySize = rankTexts.Length;
        for (int i = 0; i < rankTexts.Length; i++)
        {
            rankTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = rankTexts[i];
        }
        controllerSO.ApplyModifiedProperties();

        PanelToggle closeToggle = panelGO.AddComponent<PanelToggle>();
        SerializedObject closeToggleSO = new SerializedObject(closeToggle);
        closeToggleSO.FindProperty("panel").objectReferenceValue = panelGO;
        closeToggleSO.ApplyModifiedProperties();
        UnityEventTools.AddPersistentListener(closeButton.onClick, closeToggle.Close);

        panelGO.SetActive(false);
        return panelGO;
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

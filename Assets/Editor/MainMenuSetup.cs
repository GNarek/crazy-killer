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
        if (GameObject.Find("MainMenuCanvas") != null) return;

        GameObject canvasGO = new GameObject("MainMenuCanvas", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        canvasGO.AddComponent<GraphicRaycaster>();

        SceneSetup.EnsureEventSystem();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject titleGO = SceneSetup.CreateUIText("Title", canvasGO.transform, defaultFont, "CRAZY KILLER", 90, TextAnchor.MiddleCenter);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.65f);
        titleRT.anchorMax = new Vector2(0.5f, 0.65f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta = new Vector2(900f, 300f);

        Button playButton = SceneSetup.CreateButton("PlayButton", canvasGO.transform, defaultFont, "PLAY",
            new Color(0.2f, 0.6f, 0.9f, 1f), new Vector2(0f, 0f), new Vector2(400f, 140f));

        MainMenuController menuController = canvasGO.AddComponent<MainMenuController>();
        UnityEventTools.AddPersistentListener(playButton.onClick, menuController.PlayGame);

        SceneSetup.CreateMuteToggle("MuteToggle", canvasGO.transform, defaultFont, new Vector2(0f, -220f));
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

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class AndroidBuildSetup
{
    [MenuItem("Tools/Crazy Killer/Configure Android Build Settings")]
    public static void ConfigureAndroidSettings()
    {
        PlayerSettings.companyName = "GNarek";
        PlayerSettings.productName = "Crazy Killer";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.gnarek.crazykiller");

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

        EditorUserBuildSettings.development = true;

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        EditorUtility.DisplayDialog("Android Build Configured",
            "Package: com.gnarek.crazykiller\nMin SDK: Android 7.0 (API 24)\nOrientation: Portrait\nScripting: Mono (fast iteration builds)\n\nNote: before publishing to Google Play, switch to IL2CPP + ARM64 (Play Store requires 64-bit).\n\nNow connect your phone via USB and use File > Build Settings > Build And Run.",
            "OK");
    }
}

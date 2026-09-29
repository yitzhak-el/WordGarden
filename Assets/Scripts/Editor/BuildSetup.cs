#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WordGarden.EditorTools
{
    public static class BuildSetup
    {
        [MenuItem("Word Garden/Prepare mobile build")]
        public static void Prepare()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/WordGarden.unity", true) };
            PlayerSettings.companyName = "Word Garden";
            PlayerSettings.productName = "Word Garden";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.wordgarden.learnenglish");
            Debug.Log("Portrait scene registered. Review package identifier and signing before release.");
        }
    }
}
#endif

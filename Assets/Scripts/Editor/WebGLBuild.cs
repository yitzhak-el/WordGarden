#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WordGarden.EditorTools
{
    public static class WebGLBuild
    {
        // Optional manual build: Word Garden > Build WebGL. CI uses GameCI's default builder.
        [MenuItem("Word Garden/Build WebGL locally")]
        public static void Build()
        {
            var scenes = new[] { "Assets/Scenes/WordGarden.unity" };
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "build/WebGL/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("WebGL build failed: " + report.summary.result);
            Debug.Log("WebGL build at build/WebGL/WebGL");
        }
    }
}
#endif

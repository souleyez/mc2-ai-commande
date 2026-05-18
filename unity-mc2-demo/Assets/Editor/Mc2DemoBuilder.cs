using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MC2Demo.EditorTools
{
    public static class Mc2DemoBuilder
    {
        public static void BuildWindows64()
        {
            Mc2DemoSceneBuilder.RebuildDemoScene();
            Mc2DemoValidator.ValidateMissionContractWithoutExit();

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Windows");
            Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new()
            {
                scenes = new[] { "Assets/Scenes/Mc2Demo.unity" },
                locationPathName = Path.Combine(outputDir, "MC2UnityDemo.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException("Windows build failed: " + report.summary.result);
            }

            Debug.Log("MC2 Unity demo Windows build OK: " + options.locationPathName);
            EditorApplication.Exit(0);
        }
    }
}

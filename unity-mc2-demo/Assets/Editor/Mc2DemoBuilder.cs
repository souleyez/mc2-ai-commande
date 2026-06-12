using System.IO;
using System.Linq;
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

        public static void BuildAndroid()
        {
            string androidPlayerPath = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
            if (!Directory.Exists(androidPlayerPath))
            {
                throw new System.InvalidOperationException("Android Build Support is not installed. Expected Unity module folder: " + androidPlayerPath);
            }
            ConfigureAndroidTools(androidPlayerPath);
            ConfigureMobileLandscapePlayerSettings();

            Mc2DemoSceneBuilder.RebuildDemoScene();
            Mc2DemoValidator.ValidateMissionContractWithoutExit();

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android");
            Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new()
            {
                scenes = new[] { "Assets/Scenes/Mc2Demo.unity" },
                locationPathName = Path.Combine(outputDir, "MC2UnityDemo.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException("Android build failed: " + report.summary.result);
            }

            Debug.Log("MC2 Unity demo Android build OK: " + options.locationPathName);
            EditorApplication.Exit(0);
        }

        private static void ConfigureAndroidTools(string androidPlayerPath)
        {
            string sdkPath = Path.Combine(androidPlayerPath, "SDK");
            string ndkPath = Path.Combine(androidPlayerPath, "NDK");
            string jdkPath = Path.Combine(androidPlayerPath, "OpenJDK");
            string gradlePath = Path.Combine(androidPlayerPath, "Tools", "gradle");

            RequireDirectory(sdkPath, "Android SDK");
            RequireDirectory(ndkPath, "Android NDK");
            RequireDirectory(jdkPath, "Android OpenJDK");
            RequireDirectory(gradlePath, "Android Gradle");

            System.Type settingsType = System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEditor.Android.AndroidExternalToolsSettings", false))
                .FirstOrDefault(type => type != null);
            if (settingsType == null)
            {
                throw new System.InvalidOperationException("UnityEditor.Android.AndroidExternalToolsSettings is unavailable. Reinstall Android Build Support for this Unity editor.");
            }

            SetAndroidToolPath(settingsType, "sdkRootPath", sdkPath);
            SetAndroidToolPath(settingsType, "ndkRootPath", ndkPath);
            SetAndroidToolPath(settingsType, "jdkRootPath", jdkPath);
            SetAndroidToolPath(settingsType, "gradlePath", gradlePath);

            Debug.Log("MC2 Android tools configured: SDK=" + sdkPath + " NDK=" + ndkPath + " JDK=" + jdkPath + " Gradle=" + gradlePath);
        }

        private static void RequireDirectory(string path, string label)
        {
            if (!Directory.Exists(path))
            {
                throw new System.InvalidOperationException(label + " is not installed. Expected folder: " + path);
            }
        }

        private static void SetAndroidToolPath(System.Type settingsType, string propertyName, string path)
        {
            System.Reflection.PropertyInfo property = settingsType.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (property == null || !property.CanWrite)
            {
                throw new System.InvalidOperationException("Android external tools property is unavailable: " + propertyName);
            }

            property.SetValue(null, path);
        }

        private static void ConfigureMobileLandscapePlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            Debug.Log("MC2 mobile orientation configured: landscape-only touch build");
        }
    }
}

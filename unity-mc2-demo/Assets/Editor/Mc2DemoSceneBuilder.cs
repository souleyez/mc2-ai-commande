using MC2Demo.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MC2Demo.EditorTools
{
    public static class Mc2DemoSceneBuilder
    {
        [MenuItem("MC2 Demo/Rebuild Demo Scene")]
        public static void RebuildDemoScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new("MC2 Demo Bootstrap");
            root.AddComponent<Mc2DemoBootstrap>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Mc2Demo.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Mc2Demo.unity", true)
            };

            AssetDatabase.SaveAssets();
        }
    }
}

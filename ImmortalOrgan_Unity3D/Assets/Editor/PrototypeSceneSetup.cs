#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImmortalOrgan.EditorTools
{
    /// <summary>Generates a clean build scene during the first Unity import.</summary>
    [InitializeOnLoad]
    public static class PrototypeSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/ImmortalOrgan_Prototype.unity";

        static PrototypeSceneSetup()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        [MenuItem("Immortal Organ/1 · Open playable scene")]
        public static void OpenScene()
        {
            EnsureScene();
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        /// <summary>
        /// Call from Unity Build Automation's pre-export hook. The prototype
        /// scene is generated in the Editor, so it must exist before a clean
        /// cloud checkout starts building its Android player.
        /// </summary>
        public static void EnsureSceneForBuild()
        {
            EnsureScene();
            if (!File.Exists(ScenePath))
                throw new FileNotFoundException("Unable to prepare the Immortal Organ build scene", ScenePath);
        }

        private static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory("Assets/Scenes");
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("IMMORTAL ORGAN | Scene Bootstrap").AddComponent<GameRuntime>();
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log("[Immortal Organ] Prototype scene created. Press Play to start.");
            }

            bool present = false;
            foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            {
                if (s.path == ScenePath && s.enabled) present = true;
            }
            if (!present)
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.companyName = "Kubei Studio";
            PlayerSettings.productName = "The Immortal Organ Prototype";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,
                "com.kubei.immortalorgan.prototype");
        }

        [MenuItem("Immortal Organ/2 · Build Android debug APK")]
        public static void BuildAndroid()
        {
            EnsureScene();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUtility.DisplayDialog("Android 빌드 준비",
                    "File > Build Profiles에서 Android로 전환하고 Android Build Support를 설치한 뒤 다시 시도하세요.",
                    "확인");
                return;
            }

            Directory.CreateDirectory("Builds");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/ImmortalOrgan_Android_Debug.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log("[Immortal Organ] Android APK ready at Builds/ImmortalOrgan_Android_Debug.apk");
            else Debug.LogError("[Immortal Organ] APK build failed. Inspect the Unity Console and Build Report.");
        }
    }
}
#endif

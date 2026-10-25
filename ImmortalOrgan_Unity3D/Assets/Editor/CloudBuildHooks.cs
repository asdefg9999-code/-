#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ImmortalOrgan.EditorTools
{
    /// <summary>
    /// Unity Build Automation Advanced settings -> Pre-export method:
    /// ImmortalOrgan.EditorTools.CloudBuildHooks.PreExport
    /// </summary>
    public sealed class CloudBuildHooks : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public static void PreExport()
        {
            PrototypeSceneSetup.EnsureSceneForBuild();
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.useAPKExpansionFiles = false;
            Debug.Log("[Immortal Organ] Cloud Build ready: Android APK, prototype scene registered.");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android) return;
            PreExport();
        }
    }
}
#endif

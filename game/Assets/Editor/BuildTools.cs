using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Prpg.EditorTools
{
    /// <summary>
    /// Run from build.bat in batch mode, with no Unity window:
    ///   Setup   - makes the (empty) scene and sets the player settings. A separate run, so the build that follows
    ///             starts with those settings in place.
    ///   Windows - builds build/PRPG.exe and copies data/ and the art next to it.
    /// </summary>
    public static class BuildTools
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        static string Repo => Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));

        public static void Setup()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
                if (!File.Exists(Path.Combine(Application.dataPath, "Scenes", "Main.unity")))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, ScenePath);      // the game builds itself from code at start-up
                }
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

                PlayerSettings.companyName = "Mount Baker Games";
                PlayerSettings.productName = "PRPG";
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerSettings.resizableWindow = true;
                PlayerSettings.runInBackground = false;
                PlayerSettings.visibleInBackground = true;
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

                // Input: the Input System package only (1). Set through the serialized settings; there is no public API.
                var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (settings != null && settings.Length > 0)
                {
                    var so = new SerializedObject(settings[0]);
                    var prop = so.FindProperty("activeInputHandler");
                    if (prop != null && prop.intValue != 1) { prop.intValue = 1; so.ApplyModifiedPropertiesWithoutUndo(); }
                }
                AssetDatabase.SaveAssets();
                Debug.Log("PRPG setup done.");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        public static void Windows()
        {
            try
            {
                string outDir = Path.Combine(Repo, "build");
                if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = Path.Combine(outDir, "PRPG.exe"),
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None,
                };
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("PRPG build failed: " + report.summary.result + ", " + report.summary.totalErrors + " errors.");
                    EditorApplication.Exit(1);
                    return;
                }
                // Data and art are read from files next to the game, so the tuning panel can write numbers back.
                CopyDir(Path.Combine(Repo, "data"), Path.Combine(outDir, "data"));
                foreach (var who in new[] { "lucca", "nina", "ramorand" })
                    CopyDir(Path.Combine(Repo, "assets", "sprites", who, "atlas"), Path.Combine(outDir, "assets", "sprites", who, "atlas"));
                CopyDir(Path.Combine(Repo, "assets", "sprites", "items"), Path.Combine(outDir, "assets", "sprites", "items"), topOnly: true);
                Debug.Log("PRPG build done: " + options.locationPathName);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        static void CopyDir(string from, string to, bool topOnly = false)
        {
            Directory.CreateDirectory(to);
            foreach (var f in Directory.GetFiles(from)) File.Copy(f, Path.Combine(to, Path.GetFileName(f)), true);
            if (topOnly) return;
            foreach (var d in Directory.GetDirectories(from)) CopyDir(d, Path.Combine(to, Path.GetFileName(d)));
        }
    }
}

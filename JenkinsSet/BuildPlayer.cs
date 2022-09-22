using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public static class BuildPlayer
{

    public static string[] SCENES = FindEnabledEditorScenes();
    public static string APP_NAME = "arisu_";
    public static string TARGET_DIR = "apkbuild";

    // 빌드 실행시 사용 함수 BuildAndroid()
    [MenuItem("Build/Android APK Build")]
    public static void BuildAndroid()
    {
        string fileName = string.Empty;

#if RELEASE
        fileName = $"{APP_NAME}_Release.apk";
#else
        fileName = $"{APP_NAME}_Debug.apk";
#endif
        // keyaliaPass = akqmffjtm!
        // keystorePass = akqmffjtmdkfltn!
        //PlayerSettings.keyaliasPass = GetArg("akqmffjtm!");
        //PlayerSettings.keystorePass = GetArg("akqmffjtmdkfltn!");

        PlayerSettings.keyaliasPass = GetArg("-keyaliasPass");
        PlayerSettings.keystorePass = GetArg("-keystorePass");

        EditorUserBuildSettings.buildAppBundle = false;

        string strOutputDir = string.Format("{0}/{1}", Directory.GetCurrentDirectory(), TARGET_DIR);
        if (Directory.Exists(strOutputDir) == false)
        {
            var di = Directory.CreateDirectory(strOutputDir);
            if (di != null)
            {
                Debug.Log($"Make output Dir =>({di.FullName})");
            }
            else
            {
                Debug.Log("Directory is null");
            }
        }
        else
        {
            Debug.Log($"Make output Dir Exists , strOutputDir is {strOutputDir}");
        }

        GenericBuild(SCENES, strOutputDir + @"\" + fileName, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Build/Build IOS")]
    public static void Build_IOS()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = FindEnabledEditorScenes();
        buildPlayerOptions.locationPathName = "Build(IOS)";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded : " + summary.totalSize + "bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }


    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    static void GenericBuild(string[] scenes, string target_dir, BuildTargetGroup build_group, BuildTarget build_target, BuildOptions build_options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_group, build_target);
        BuildReport report = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            //System.Diagnostics.Process.Start(target_dir);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed: " + summary.totalErrors + " erros");
        }
    }

    public static string GetArg(string key)
    {
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (key.Equals(args[i]))
            {
                return args[i + 1];
            }
        }

        Debug.LogError($"GetArg :: {key} is null");
        return string.Empty;
    }
}


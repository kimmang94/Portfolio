using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;
//using Newtonsoft.Json;
//using Google.Android.AppBundle.Editor.Internal.AssetPacks;
using System.Linq;

public class Builder
{
    // Debug용, Release 용 
    public enum BuildType
    {
        DEBUG = 0,
        RELEASE
    }

    // APP 정보
    public static string[] SCENES = FindEnabledEditorScenes();
    public static string APP_NAME = "meemz_";
    public static string TARGET_DIR = "apkbuild";

    public static void ChangeDefineSymbols(BuildType type)
    {
        List<string> defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android).Split(';').ToList();

        defineSymbols.Remove("DEBUG");
        defineSymbols.Remove("RELEASE");

        string newDefineSymbols = string.Empty;
        for (int i = 0; i < defineSymbols.Count; i++)
        {
            newDefineSymbols += $"{defineSymbols[i]};";
        }

        newDefineSymbols += $"{type.ToString()};";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newDefineSymbols);
    }

    // Debug빌드 세팅
    [MenuItem("Build/Set Debug Server")]
    public static void SetDebugBuild()
    {
        //Photon.Pun.ServerSettings PhotonServerSettings = Resources.Load<Photon.Pun.ServerSettings>("PhotonServerSettings");
        EditorUserBuildSettings.buildAppBundle = false;
        //AssetDeliveryConfig assetDeliveryConfig = AssetDeliveryConfigSerializer.LoadConfig();
        //assetDeliveryConfig.SplitBaseModuleAssets = false;
        //assetDeliveryConfig.Refresh();
        //PhotonServerSettings.AppSettings.AppIdRealtime = "dab3acfc-a117-4141-8054-745dde7e8c5c";
        //PhotonServerSettings.AppSettings.AppIdChat = "6db2212f-5796-4a8b-a8ed-e19aa63af4c9";

        ChangeDefineSymbols(BuildType.DEBUG);
    }
    // Release 서버용 세팅
    [MenuItem("Build/Set Release Server")]
    public static void SetReleaseBuild()
    {
        //Photon.Pun.ServerSettings PhotonServerSettings = Resources.Load<Photon.Pun.ServerSettings>("PhotonServerSettings");
        EditorUserBuildSettings.buildAppBundle = true;
        //AssetDeliveryConfig assetDeliveryConfig = AssetDeliveryConfigSerializer.LoadConfig();
        //assetDeliveryConfig.SplitBaseModuleAssets = true;
        //assetDeliveryConfig.Refresh();
        //PhotonServerSettings.AppSettings.AppIdRealtime = "8db0b96a-f119-4220-a82c-6637b0b990fd";
        //PhotonServerSettings.AppSettings.AppIdChat = "6db2212f-5796-4a8b-a8ed-e19aa63af4c9";

        ChangeDefineSymbols(BuildType.RELEASE);
    }

    // 안드로이드 APK 빌드 세팅
    [MenuItem("Build/Android APK Build")]
    public static void BuildAndroid()
    {
        string fileName = string.Empty;

#if RELEASE
        fileName = $"{APP_NAME}_Release.apk";
#else
        fileName = $"{APP_NAME}_Debug.apk";
#endif
        // KeystorePass
        PlayerSettings.keystorePass = "akqmffjtm!";
        // KeystoreAliasPass (요거를 변경)
        PlayerSettings.keyaliasPass = "akqmffjtm!";
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
    // 이건 AAB 빌드
    [MenuItem("Build/Android AAB Build")]
    public static void BuildReleaseAndroid()
    {
        string fileName = string.Empty;
#if RELEASE
        fileName = $"{APP_NAME}_Release.aab";
#else
        fileName = $"{APP_NAME}_Debug.aab";
#endif

        PlayerSettings.keystorePass = "akqmffjtm!";
        PlayerSettings.keyaliasPass = "akqmffjtm!";
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

    // 빌드 버전하구 넘버 정보
    private class BuildInfo
    {
        public string BuildVersion { get; set; }
        public int BuildNumber { get; set; }
    }

    private static void IncreaseBundleVersion()
    {
        var path = "";

        string[] arr = AssetDatabase.FindAssets("BuildInfo");
        if (arr.Length > 0)
        {
            path = AssetDatabase.GUIDToAssetPath(arr[0]);
            TextAsset jsonAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            if (jsonAsset != null)
            {
                string strJSON = jsonAsset.text;
                //BuildInfo info = JsonConvert.DeserializeObject<BuildInfo>(strJSON);
                //PlayerSettings.bundleVersion = info.BuildVersion;
                //PlayerSettings.Android.bundleVersionCode = info.BuildNumber;
            }
            else
            {
                Debug.Log("--------IncreaseBundleVersion--------");
            }
        }
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

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }
}

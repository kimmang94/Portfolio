using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/*
    Unity 용 KeyStore 생성 해야함
    Version, WholeManager Version, Bundle Version Code 모두 숫자 올려서 빌드할것!
    기존 Debug KeyStore 는 보존, 나중에 다시 자신의 디버그 키로 빌드할 시 변경
    OpenCV 관련 데이터는 제거 (경로 문제)
    빌드하기전 Contetns 폴더, temp폴더와 versions.txt 제거
    MainManager.cs 설정 (Release, Daekyo, 체험판(Demo))
    Release 체크 되면 성장판 앱 필요, 체크 안하면 단독으로 실행 가능
    Release / Daekyo 에서는 FireBase 초기화
    체험판 에서는 FireBase 초기화 삭제
    카네기 패스경로 string _aaZipPath = "http://deaol6osmomy7.cloudfront.net/mainContents/Carnegie/20210524/aa.zip";
    Release : 써밋\본앱\StreamingAssets 폴더 복사
    POC : 써밋\POC\StreamingAssets 폴더 복사
    
 */

public class Build : MonoBehaviour
{

    public static string[] SCENES = FindEnabledEditorScenes();
    public static string APP_NAME = "SpeakitDaekyo";
    public static string TARGET_DIR = "apkbuild";
    public static string VERSION = "";
    // 빌드 실행시 사용 함수 BuildAndroid()
    [MenuItem("Build/Android APK Build")]
    public static void BuildAndroid()
    {
        string fileName = string.Empty;
        DeleteFile();
#if RELEASE
        fileName = $"{APP_NAME}_Release.apk";
        VERSION = WholeManager.Release.version;
#elif Daekyo
        fileName = $"{APP_NAME}_Daekyo.apk";
        VERSION = WholeManager.Daekyo.version;
#else
        fileName = $"{APP_NAME}_Demo.apk";
        //VERSION = WholeManager.Demo.version;
#endif
        //PlayerSettings.keyaliasPass = GetArg("akqmffjtm!");
        //PlayerSettings.keystorePass = GetArg("speakitdaekyo!");

        PlayerSettings.keyaliasPass = GetBuildCommand("keyaliasPass");
        PlayerSettings.keystorePass = GetBuildCommand("keystorePass");

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

    public static Dictionary<string, string> buildCommand = new Dictionary<string, string>();
    public static void GetCommandLine()
    {
        var args = System.Environment.GetCommandLineArgs();
        buildCommand = new Dictionary<string, string>();
        /**
         * args[0] = -projectPath
         * args[1] = {Proejct_Path}
         * args[2] = -quit
         * args[3] = -batchMode
         * args[4] = -logFile
         * args[5] = {LogFilePath}
         * args[6] = -executeMethod
         * args[7] = {ExcutedMethod}
         *  0 ~ 7 pass...
         **/

        for (int i = 8; i < args.Length; i += 2)
        {
            string key = args[i].Replace("-", "");

            if (string.IsNullOrEmpty(args[i + 1]))
            {
                buildCommand.Add(key, string.Empty);
                continue;
            }

            string value = args[i + 1];

            buildCommand.Add(key, value);
        }
    }

    public static string GetBuildCommand(string key)
    {
        if (buildCommand.ContainsKey(key))
        {
            return buildCommand[key];
        }
        else
        {
            Debug.LogError($"GetBuildCommnad :: {key} is empty...");
            return string.Empty;
        }
    }

    public static void DeleteFile()
    {
        System.IO.File.Delete("StreamingAssets/Contents");
        System.IO.File.Delete("StreamingAssets/temp");
        System.IO.File.Delete("StreamingAssets/versions.txt");
    }
}




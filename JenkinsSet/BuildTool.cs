using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditor.AddressableAssets.Settings;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.Text;
//using UnityEditor.AddressableAssets.Build;
using System.IO;
//using Newtonsoft.Json.Linq;
//using UnityEditor.AddressableAssets;

public class BuildTool : EditorWindow
{
    public static Dictionary<string, AddressableVersion> BuildConfig
    {
        get; private set;
    }

    #region Build Config
    public class AddressableVersion
    {
        public int Android;
        public int iOS;
        public string Build_PC;
        public string Build_Time;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Android :: {Android}")
                .AppendLine($"iOS :: {iOS}")
                .AppendLine($"Build_PC :: {Build_PC}")
                .AppendLine($"Build_Time :: {Build_Time}");

            return sb.ToString();
        }
    }
    #endregion

    public static string currentAppVersion;
    public static string buildAppVersion;
    public static string configLoaclPath;
    public static int buildAAVersion;

    [MenuItem("Build/Open BuildTool", false, 0)]
    static void Init()
    {
        configLoaclPath = $"{Application.persistentDataPath}/buildconfig.json";

        BuildTool window = (BuildTool)GetWindow(typeof(BuildTool));
        window.minSize = new Vector2(430, 400);
        window.maxSize = new Vector2(430, 800);
        window.Show();


        UpdateView();
    }

    private void OnGUI()
    {
        #region update.
        GUILayout.Space(20);
        if (GUILayout.Button("BuildConfig 갱신하기", GUILayout.Height(30)))
        {
            UpdateView();
        }

        if (BuildConfig is null)
            return;
        #endregion

        #region App Build.
        GUILayout.Space(20);
        GUILayout.Label("================[ Application Build Part ]================");
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"현재 Application 버전: ");
        EditorGUILayout.LabelField($"{currentAppVersion}");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("빌드 Application 버전: ");
        buildAppVersion = EditorGUILayout.TextField(buildAppVersion);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Debug APK", GUILayout.Height(30)))
        {
            ShowDialog("Build APK", $"Build Debug APK :: {buildAppVersion}", () =>
            {
                Builder.SetDebugBuild();
                Builder.BuildAndroid();
            }, () =>
            {
                Debug.Log("Cancel");
            });
        }
        if (GUILayout.Button("Build Release APK", GUILayout.Height(30)))
        {
            ShowDialog("Build APK", $"Build Release APK :: {buildAppVersion}", () =>
            {
                Builder.SetReleaseBuild();
                Builder.BuildAndroid();
            }, () =>
            {
                Debug.Log("Cancel");
            });
        }
        if (GUILayout.Button("Build Release AAB", GUILayout.Height(30)))
        {
            ShowDialog("Build AAB", $"Build Release AAB :: {buildAppVersion}", () =>
            {
                Builder.SetReleaseBuild();
                Builder.BuildReleaseAndroid();
            }, () =>
            {
                Debug.Log("Cancel");
            });
        }
        GUILayout.EndHorizontal();
        #endregion


        #region Addressable Asset
        GUILayout.Space(40);
        GUILayout.Label("===============[ AddressableAsset Build Part ]===============");
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("현재 Addressable 버전: ");
        if (BuildConfig.ContainsKey(buildAppVersion) is false)
        {
            EditorGUILayout.LabelField($"-");
        }
        else
        {
            EditorGUILayout.LabelField($"{BuildConfig[buildAppVersion].Android}");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("빌드 Addressable 버전: ");
        buildAAVersion = EditorGUILayout.IntField(buildAAVersion);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Local Addressable", GUILayout.Height(30)))
        {
            ShowDialog("Build Local Addressable", $"Build Local Addressable\nVersion: {buildAppVersion}.{buildAAVersion}", () =>
            {
                BuildAddressableAssets(buildAppVersion, buildAAVersion, false, () =>
                {
                    if (BuildConfig.ContainsKey(buildAppVersion) is false)
                    {
                        BuildConfig.Add(buildAppVersion, new AddressableVersion());
                    }

                    BuildConfig[buildAppVersion].Android = buildAAVersion;
                    BuildConfig[buildAppVersion].Build_PC = System.Environment.MachineName;
                    BuildConfig[buildAppVersion].Build_Time = DateTime.Now.ToString();
                });
            }, () =>
            {
            });
        }
        if (GUILayout.Button("Build Addressable & upload S3", GUILayout.Height(30)))
        {
            ShowDialog("Build Addressable & upload S3", $"Build Addressable\n==> S3 Upload\n==> BuildConfig Update\nVersion: {buildAppVersion}.{buildAAVersion}", () =>
            {
                BuildAddressableAssets(buildAppVersion, buildAAVersion, true, () =>
                {
                    if (BuildConfig.ContainsKey(buildAppVersion) is false)
                    {
                        BuildConfig.Add(buildAppVersion, new AddressableVersion());
                    }

                    BuildConfig[buildAppVersion].Android = buildAAVersion;
                    BuildConfig[buildAppVersion].Build_PC = System.Environment.MachineName;
                    BuildConfig[buildAppVersion].Build_Time = DateTime.Now.ToString();

                    UploadConfig();
                });
            }, () =>
            {
            });
        }

        GUILayout.EndHorizontal();
        #endregion
    }

    private void OnDestroy()
    {
        BuildConfig = null;
    }

    static void ShowDialog(string _title, string _desc, Action _ok, Action _cancel)
    {
        if (EditorUtility.DisplayDialog(_title, _desc, "OK", "Cancel"))
        {
            _ok?.Invoke();
        }
        else
        {
            _cancel?.Invoke();
        }
    }

    static void BuildAddressableAssets(string _appVersion, int _AAVersion, bool _upload, Action _callback)
    {
        if (Directory.Exists($"{Application.dataPath}/../ServerData/Android"))
        {
            Directory.Delete($"{Application.dataPath}/../ServerData/Android", true);
        }

        //AddressableAssetSettings settings = 

        //var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Packed Assets");
        //var path = ContentUpdateScript.GetContentStateDataPath(false);
        //var result = ContentUpdateScript.BuildContentUpdate(group.Settings, path);

        //if (result is null)
        //{
        //    Debug.LogError("Addressable Build Faile...");
        //    return;
        //}

        //if (string.IsNullOrEmpty(result.Error))
        //{
        //    Debug.Log($"Addressable Build Success !!");

        //    if (_upload)
        //    {
        //        _ = S3Uploader.UploadDirectory($"{Application.dataPath}/../ServerData/Android", $"AddressableAssets/{_appVersion}/{_AAVersion}", "Android", () =>
        //        {
        //            Debug.Log($"Addressable S3 upload Success !!");
        //            _callback?.Invoke();
        //        });
        //    }
        //    else
        //    {
        //        _callback?.Invoke();
        //    }
        //}
        //else
        //{
        //    Debug.LogError("Addressable Build Faile...");
        //}
    }

    static void UpdateView()
    {
        BuildConfig = null;

        _ = DownloadBuildConfig(() =>
        {
            currentAppVersion = Application.version;
            buildAppVersion = Application.version;
        });
    }

    static async Task DownloadBuildConfig(Action _complete)
    {
        string configUrl = "https://meemz-addressables.s3.us-east-1.amazonaws.com/Config/buildconfig.json";

        var request = UnityWebRequest.Get(configUrl);
        var result = request.SendWebRequest();

        while (!result.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Get build config faile... retry... {request.error}");
            _ = DownloadBuildConfig(_complete);
            return;
        }
        else
        {
            File.WriteAllBytes(configLoaclPath, request.downloadHandler.data);
            Debug.Log($"Write Build Config:: {configLoaclPath}");
            Debug.Log(request.downloadHandler.text);
            UpdateBuiildConfig();
            _complete?.Invoke();
            //JObject jData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
            //BuildConfig = JsonConvert.DeserializeObject<AddressableVersion>(jData[$"{Application.version}"].ToString());
        }
    }

    static void UpdateBuiildConfig()
    {
        BuildConfig = new Dictionary<string, AddressableVersion>();
        string data = File.ReadAllText(configLoaclPath);
        //BuildConfig = JsonConvert.DeserializeObject<Dictionary<string, AddressableVersion>>(data);
    }

    static void UploadConfig()
    {
        //var data = JsonConvert.SerializeObject(BuildConfig);
        //File.WriteAllText(configLoaclPath, data);
        //Debug.Log($"Write buildconfig :: {configLoaclPath} :: {data}");

        //_ = S3Uploader.UploadConfigAsync(configLoaclPath, "buildconfig.json", null);
    }
}
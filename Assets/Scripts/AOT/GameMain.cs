using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using UniFramework.Event;
using YooAsset;

public class GameMain : MonoBehaviour
{
    private const string mYooDefaultPKG = "DefaultPackage";
    private const string mYooHotFixPKG = "HotFixPackage";
    //PatchedAOTAssemblyList--加载时会追加 *.bytes
    public static readonly List<string> PatchedAOTAssemblyList = new List<string>
    {
         "AOT.dll",
        "System.Core.dll",
        "UniFramework.Event.dll",
        "UnityEngine.CoreModule.dll",
        "YooAsset.dll",
        "mscorlib.dll",
        //
        "HotUpdate.dll",
    };

    /// <summary>    /// 资源系统运行模式    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    public static GameMain Instance;

    void Awake()
    {
        Instance = this;
        Debug.LogWarning($"资源系统运行模式：{PlayMode}");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);
    }

    IEnumerator Start()
    {
        // 初始化事件系统
        UniEvent.Initalize();

        // 初始化资源系统
        YooAssets.Initialize();

        //// 加载更新页面
        //var go = Resources.Load<GameObject>("PatchWindow");
        //GameObject.Instantiate(go);
#if UNITY_EDITOR
        yield return LoadEditorSimulate();
#else
        yield return LoadGameHotFix();
#endif

        var uiBinder = _hotUpdateAss.GetType("UIGenBinder");
        uiBinder.GetMethod("BindAll").Invoke(null, null);

        var hotFixReflex = _hotUpdateAss.GetType("HotFixReflex");
        hotFixReflex.GetMethod("Run").Invoke(null, null);

        // 设置默认的资源包
        var gamePackage = YooAssets.GetPackage(mYooDefaultPKG);
        YooAssets.SetDefaultPackage(gamePackage);
    }
    /// <summary>  不走热更流程       Editor下无需加载，直接查找获得HotUpdate程序集  </summary>
    private IEnumerator LoadEditorSimulate()
    {
        var pkg = YooAssets.CreatePackage(mYooDefaultPKG);
        YooAssets.SetDefaultPackage(pkg);
        var editorMode = new EditorSimulateModeParameters();
        editorMode.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), mYooDefaultPKG);
        var initAsync = pkg.InitializeAsync(editorMode);
        yield return initAsync;
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
    }
    /// <summary>  走热更流程       1两个包的补丁 2元数据与代码 3设置默认包  </summary>
    private IEnumerator LoadGameHotFix()
    {
        // 开始补丁更新流程
        var operation_default = new PatchOperation(mYooDefaultPKG, EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), PlayMode);
        YooAssets.StartOperation(operation_default);
        yield return operation_default;

        //更新热更代码
        var operation_hotFix = new PatchOperation(mYooHotFixPKG, EDefaultBuildPipeline.RawFileBuildPipeline.ToString(), PlayMode);
        YooAssets.StartOperation(operation_hotFix);
        yield return operation_hotFix;

        //加载 元数据 和 热更新代码
        yield return LoadHotFixRes();
        LoadMetadataForAOTAssebly();

        var pkg = YooAssets.CreatePackage(mYooDefaultPKG);
        YooAssets.SetDefaultPackage(pkg);
    }




    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] GetAssetData(string dllName)
    {
        if (s_assetDatas.TryGetValue(dllName, out var data))
        {
            return data;
        }

        return null;
    }

    private static Assembly _hotUpdateAss;

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void LoadMetadataForAOTAssebly()
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in PatchedAOTAssemblyList)
        {
            var dllBytes = GetAssetData(aotDllName);
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadAOT:{aotDllName} mode:{mode}  ret:{err}");
        }
        _hotUpdateAss = Assembly.Load(GetAssetData($"HotUpdate.dll"));//仅热更时才能走到这个方法

        //#if UNITY_EDITOR // Editor下无需加载，直接查找获得HotUpdate程序集
        //        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
        //#else
        //        _hotUpdateAss = Assembly.Load(GetAssetData($"HotUpdate.dll"));
        //#endif
    }

    private IEnumerator LoadHotFixRes()
    {
        var hotPKG = YooAssets.GetPackage("HotFixPackage");
        foreach (var dll in PatchedAOTAssemblyList)
        {
            var handle = hotPKG.LoadRawFileAsync($"Assets/GameResHotFix/{dll}.bytes");
            yield return handle;
            var bytes = handle.GetRawFileData();
            s_assetDatas[dll] = bytes;
        }
    }
}
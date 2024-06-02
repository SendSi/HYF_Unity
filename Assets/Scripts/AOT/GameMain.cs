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
    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    public static GameMain Instance;

    void Awake()
    {
        Instance = this;
        Debug.Log($"资源系统运行模式：{PlayMode}");
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

        // 加载更新页面
        var go = Resources.Load<GameObject>("PatchWindow");
        GameObject.Instantiate(go);

        // 开始补丁更新流程
        PatchOperation operation = new PatchOperation("DefaultPackage", EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), PlayMode);
        YooAssets.StartOperation(operation);
        yield return operation;

        //加載 元數據 和熱更新代碼
        yield return LoadHotFixRes();
        LoadMetadataForAOTAssebly();

        // 设置默认的资源包
        var gamePackage = YooAssets.GetPackage("DefaultPackage");
        YooAssets.SetDefaultPackage(gamePackage);
    }

    //PatchedAOTAssemblyList--加载时会追加 *.bytes
    public static readonly List<string> PatchedAOTAssemblyList = new List<string>
    {
        "UniFramework.Event.dll",
        "UnityEngine.CoreModule.dll",
        "YooAsset.dll",
        "mscorlib.dll",
        //
        "HotUpdate.dll",
    };

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
        
#if UNITY_EDITOR // Editor下无需加载，直接查找获得HotUpdate程序集
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#else
        _hotUpdateAss = Assembly.Load(GetAssetData($"HotUpdate.dll"));
#endif
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
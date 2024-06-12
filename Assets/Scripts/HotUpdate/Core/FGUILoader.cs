using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using YooAsset;
using System;

public class FGUILoader : Singleton<FGUILoader>
{
    private int mCurTimeNum = 0; //当前运行的时间
    private const int mReleaseTime = 20; //当无引用时 多少秒后 释放
    private const string mYooDefaultPKG = "DefaultPackage";

    private readonly Dictionary<string, bool> mForeverPKG = new Dictionary<string, bool>()
    {
        ["CommonPKG"] = true,
        ["Emoji"] = true,
        ["ItemPKG"] = true,
    };

    /// <summary>     key=pkgName,,,value=fgui的pkg     </summary>
    private Dictionary<string, UIPackage> mLoadedPKG = new Dictionary<string, UIPackage>();

    /// <summary>     key=pkgName,,,value=时间     </summary>
    private Dictionary<string, int> mReleasePKGDic = new Dictionary<string, int>();

    /// <summary>     key=pkgName,,,value=已load出的yooAsset的package     </summary>
    private Dictionary<string, List<AssetHandle>> mLoadedHandles = new Dictionary<string, List<AssetHandle>>();

    protected override void OnInit()
    {
        base.OnInit();
        FairyGUI.Timers.inst.Add(1, -1, (cb) =>
        {
            mCurTimeNum += 1;
            CheckTimeReleasePKG();
        });
    }

    /// <summary> 每秒检查一次 释放包 </summary>
    private void CheckTimeReleasePKG()
    {
    }

    public void AddPackage(string pkgName, Action finishCB)
    {
        UIPackage pkgED = null;
        mReleasePKGDic.Remove(pkgName);
        if (mLoadedPKG.TryGetValue(pkgName, out pkgED))
        {
            finishCB?.Invoke();//已加载中的内存取...
            return;
        }
        //字典里没有 则去加载
        GameMain.Instance.StartCoroutine(LoadUIPackage(pkgName, finishCB));
    }

    public IEnumerator LoadUIPackage(string pkgName, Action finishCB)
    {
        var assetPKG = YooAssets.TryGetPackage(mYooDefaultPKG);
        var handle = assetPKG.LoadAssetAsync<TextAsset>($"{pkgName}_fui");//加载fgui主包的*.bytes文本
        yield return handle;
        var pkgDesc = handle.AssetObject as TextAsset;
        var tUIPKG = UIPackage.AddPackage(pkgDesc.bytes, string.Empty, (name, extension, type, pkgItem) =>
        {// public delegate void LoadResourceAsync(string name, string extension, System.Type type, PackageItem item);
            GameMain.Instance.StartCoroutine(LoadUIExtensions(pkgName, name, extension, type, pkgItem));
        });
        tUIPKG.LoadAllAssets();//加载所有
        mLoadedPKG[pkgName] = tUIPKG;//加入包中去
        TryAddHandles(pkgName, handle);

        var pkgDeepNames = GetDependencies(tUIPKG);//获得  此包的 依赖包   名字s
        GameMain.Instance.StartCoroutine(LoadDependencies(pkgDeepNames, finishCB));//去 加载依赖包
    }

    private string LoadUIExtensions(string pkgName, string name, string extension, Type type, PackageItem pkgItem)
    {
        throw new NotImplementedException();
    }

    private string LoadDependencies(List<string> pkgDeepNames, Action finishCB)
    {

    }

    private void TryAddHandles(string pkgName, AssetHandle handle)
    {

    }

    /// <summary>return pkgName所依赖的包名s </summary>
    private List<string> GetDependencies(UIPackage pkgName)
    {
        var tDepend = pkgName.dependencies;
        var num = tDepend.Length;
        var list = new List<string>();
        for (int i = 0; i < num; i++)
        {
            list.Add(tDepend[i]["name"]); //依赖包
        }

        return list;
    }
}
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using YooAsset;
using System;
using System.Linq;

public class FGUILoader : Singleton<FGUILoader>
{
    private int mCurTimeNum = 0; //当前运行的时间
    private const int mReleaseTime = 20; //当无引用时 多少秒后 释放
    private const string mYooDefaultPKG = "DefaultPackage";

    /// <summary> 常驻包(永久) 不移除销毁的  依赖公共包 </summary>
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
        for (int i = 0; i < mReleasePKGDic.Count; i++)
        {
            var item = mReleasePKGDic.ElementAt(i);
            if (item.Value <= mCurTimeNum)
            {
                UIPackage.RemovePackage(item.Key);
                mLoadedPKG.Remove(item.Key);

                ReleaseHandle(item.Key);
                Debug.LogWarning($"{item.Key} 释放了");

                mReleasePKGDic.Remove(item.Key);
            }
        }
    }

    public void RemoveUIPackage(string pkgName)
    {
        if (mForeverPKG.ContainsKey(pkgName) == false)
        {
            if (mLoadedPKG.ContainsKey(pkgName))
            {
                mReleasePKGDic[pkgName] = (mCurTimeNum + mReleaseTime);
                Debug.LogWarning($"{pkgName} 在 {mReleaseTime}秒内 再无引用 将被释放");
            }
        }
    }

    /// <summary> 把公共包 都先load出来   之后使用ui://包名/图片名  就能正常了</summary>
    public void CheckLoadCommonPKG()
    {
        var list = new List<string>();
        foreach (var item in mForeverPKG)
        {
            list.Add(item.Key);//取key值 为 列表
        }
        GameMain.Instance.StartCoroutine(LoadDependencies(list, null));
    }

    private void ReleaseHandle(string key)
    {
        if (mLoadedHandles.TryGetValue(key, out var tHandle))
        {
            foreach (var item in tHandle)
            {
                item.Release();
            }
            tHandle.Clear();
        }
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

    private IEnumerator LoadUIExtensions(string pkgName, string name, string extension, Type type, PackageItem pkgItem)
    {
        var assPkg = YooAssets.TryGetPackage(mYooDefaultPKG);
        var handle = assPkg.LoadAssetAsync($"{pkgName}_{name}");
        yield return handle;
        pkgItem.owner.SetItemAsset(pkgItem, handle.AssetObject, DestroyMethod.None);
        TryAddHandles(pkgName, handle);
    }
    /// <summary>     加载依赖包     </summary>
    private IEnumerator LoadDependencies(List<string> pkgDeepNames, Action finishCB)
    {
        var sum = pkgDeepNames.Count;
        if (sum > 0)
        {
            for (int i = 0; i < sum; i++)
            {
                var pkgName = pkgDeepNames[i];
                if (mLoadedPKG.ContainsKey(pkgName) == false)
                {
                    if (mForeverPKG.ContainsKey(pkgName))
                    {
                        var assetPkg = YooAssets.TryGetPackage(mYooDefaultPKG);
                        var handle = assetPkg.LoadAssetAsync<TextAsset>($"{pkgName}_fui");
                        yield return handle;
                        var pkgDesc = handle.AssetObject as TextAsset;
                        var tUIPkg = UIPackage.AddPackage(pkgDesc.bytes, string.Empty, (name, extension, type, pkgItem) =>
                        {
                            GameMain.Instance.StartCoroutine(LoadUIExtensions(pkgName, name, extension, type, pkgItem));
                        });
                        tUIPkg.LoadAllAssets();
                        mLoadedPKG[pkgName] = tUIPkg;
                        TryAddHandles(pkgName, handle);
                        Debug.LogWarning("加入_依赖包:" + pkgName);
                    }
                    else
                    {
                        Debug.LogError("业务包 被当成 依赖包了   加载了-->" + pkgName + ",此业务包依赖包的个数=" + sum);
                    }
                }

                if (i + 1 == sum)
                {
                    finishCB?.Invoke();
                }
            }
        }
        else
        {
            finishCB?.Invoke();
        }
    }

    private void TryAddHandles(string pkgName, AssetHandle pHandle)
    {
        if (mLoadedHandles.TryGetValue(pkgName, out var tHandle))
        {
            tHandle.Add(pHandle);
        }
        else
        {
            mLoadedHandles[pkgName] = new List<AssetHandle> { pHandle };
        }
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
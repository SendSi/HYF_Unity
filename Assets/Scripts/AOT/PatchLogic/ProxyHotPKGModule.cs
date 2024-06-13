using FairyGUI;
using HotPKG;
public class ProxyHotPKGModule : Singleton<ProxyHotPKGModule>
{
    private const string pkgName = "HotPKG";
    private HotPKG.HFView mView;

    public void OpenHFView()
    {
        UIPackage.AddPackage(pkgName);
        HotPKGBinder.BindAll();
        mView = OpenUIViewCom<HFView>(pkgName);
        mView.SetData("热更页面");
    }

    public void CloseHFView()
    {
        if (mView != null)
        {
            mView.Dispose();
            UIPackage.RemovePackage(pkgName);
        }
    }


    public T OpenUIViewCom<T>(string pkgName) where T : GComponent, new()
    {
        string viewName = (typeof(T).Name);
        var gCom = UIPackage.CreateObject(pkgName, viewName).asCom;
        gCom.MakeFullScreen();
        GRoot.inst.AddChild(gCom);
        gCom.AddRelation(GRoot.inst, RelationType.Size);
        gCom.fairyBatching = true;
        gCom.OnInit();//页面方面中  一般onClick,onClickItem,itemRenderer...在此处声明

        return gCom as T;
    }
}


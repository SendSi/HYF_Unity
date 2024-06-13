using Login;
using System;

public class ProxyLoginModule : Singleton<ProxyLoginModule>, IProxy
{
    private const string pkgName = "Login";

    public void CheckLoad(Action finishCB)
    {
        FGUILoader.Instance.AddPackage(pkgName, finishCB);
    }

    public void OpenLoginMainView()
    {
        CheckLoad(delegate
        {
            var targetView = UIMgr.Instance.OpenUIViewCom<LoginMainView>(pkgName);
            targetView.SetData("abc");
        });
    }
}

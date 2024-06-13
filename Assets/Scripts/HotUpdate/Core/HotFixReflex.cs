using UnityEngine;

#region << 脚 本 注 释 >>
//作  用:    HotFixReflex
//作  者:    曾思信
//创建时间:  #CREATETIME#
#endregion

public class HotFixReflex
{
    public static void Run()
    {
        Debug.LogWarning("HotFixReflex-->Run");
        ProxyLoginModule.Instance.OpenLoginMainView();
    }

    public static void Destroy()
    {

    }
}
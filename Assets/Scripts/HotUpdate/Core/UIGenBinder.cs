using Bag;
using CommonPKG;
using DialogTip;
using GMView;
using Login;
using MainCenter;
using MainRole;
using SettingPKG;
using UnityEngine;
using Welfare;

public class UIGenBinder
{
    public static  void BindAll()
    {
        Debug.LogWarning("--开始绑定自动生成的脚本--若没有执行OnInit(),看看此有无绑定---");
        LoginBinder.BindAll();
        MainCenterBinder.BindAll();
        DialogTipBinder.BindAll();
        BagBinder.BindAll();
        MainRoleBinder.BindAll();
        CommonPKGBinder.BindAll();
        GMViewBinder.BindAll();
        SettingPKGBinder.BindAll();
        WelfareBinder.BindAll();
    }
}
using FairyGUI;
using UnityEngine;

namespace Login
{
    public partial class LoginMainView : GComponent
    {
        public override void OnInit()
        {
            base.OnInit();
            FGUILoader.Instance.CheckLoadCommonPKG(); //加载公共依赖包
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void SetData(string data)
        {
            Debug.Log(data);
        }
    }
}

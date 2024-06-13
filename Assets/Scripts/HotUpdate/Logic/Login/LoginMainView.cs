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
            _roleInputTxt.text = "999";
            _loginBtn.onClick.Set(OnClickLoginBtn);
        }

        private void OnClickLoginBtn(EventContext context)
        {
            _roleInputTxt.text = "你建水";
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

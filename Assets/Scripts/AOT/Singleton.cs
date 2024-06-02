public class Singleton<T> where T : Singleton<T>, new()
{
    private static Singleton<T> _instance;

    public static Singleton<T> Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Singleton<T>();
                _instance.OnInit();
            }

            return _instance;
        }
    }

//继类 在此定义
    protected virtual void OnDispose()
    {
    }

    protected virtual void OnInit()
    {
    }

//调用者使用
    public void Dispose()
    {
        OnDispose();
    }
}
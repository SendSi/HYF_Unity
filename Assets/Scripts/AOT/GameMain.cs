using UnityEngine;

public class GameMain : MonoBehaviour
{
    public static GameMain Instance;
     void Awake()
    {
        Instance = this;
       
    }
     
     /// <summary>
     /// 协程启动器
     /// </summary>
     public MonoBehaviour Behaviour;
}

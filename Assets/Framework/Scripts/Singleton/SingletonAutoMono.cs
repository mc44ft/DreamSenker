using UnityEngine;

public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 注意事项

    //不用担心重复挂载问题
    //不用担心切场景的问题
    //不允许手动挂载
    //用的时候才加载到场景中

    #endregion 注意事项

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject();
                _instance = obj.AddComponent<T>();//动态添加脚本
                obj.name = typeof(T).Name + "_Instance";
                DontDestroyOnLoad(obj);//过场景不移除 保证其唯一性和游戏生命周期中的永久存在
            }
            return _instance;
        }
    }
}
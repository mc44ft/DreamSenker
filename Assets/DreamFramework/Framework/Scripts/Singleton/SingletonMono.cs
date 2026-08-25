using UnityEngine;

/// <summary>
/// 手动在场景上创建对象
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 注意事项

    //这种挂载式的单例模式基类不推荐使用 很容易破坏单例模式的唯一性
    //例如：
    //1.挂载多个相同脚本时
    //2.切换场景回来时，由于场景放置了挂载脚本的对象 回到该场景时 又会有一个该单例模式对象

    //解决方法
    ////1.在Awake中判断是否已经有实例存在，如果有则销毁当前对象

    #endregion 注意事项

    private static T _instance;

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            //如果已经有实例存在，则销毁当前脚本
            Destroy(this);
            return;
        }
        _instance = this as T;

        //过场景不移除只能作用于根物体
        //如果想给场景上所有的管理器都收纳起来，子物体的过场景不移除方法就不执行
        if(transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);//过场景不移除 保证其唯一性和游戏生命周期中的永久存在
        }
        
    }
}
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region 未限制数量

//public class poolData
//{
//    private Stack<GameObject> data = new Stack<GameObject>();
//    private GameObject root;

//    public int Count => data.Count;
//    public poolData(GameObject poolRoot, string name)
//    {
//        if (PoolMgr.isOpenLayout)
//        {
//            root = new GameObject(name);
//            root.transform.SetParent(poolRoot.transform);
//        }
//    }
//    public void Push(GameObject obj)
//    {
//        //如何隐藏该对象
//        //方式1：放入缓存池前先将对象设置为不可见
//        //方式2：将对象放置在玩家不可见的地方（老版本的Unity频繁激活失活对象会造成一定的性能开销 就使用这种方式）
//        if (PoolMgr.isOpenLayout)
//            obj.transform.SetParent(root.transform);//将对象放入根对象下 方便管理
//        obj.SetActive(false);
//        data.Push(obj);
//    }
//    public GameObject Pop()
//    {
//        GameObject obj = data.Pop();
//        obj.SetActive(true);
//        if (PoolMgr.isOpenLayout)
//            obj.transform.SetParent(null);
//        return obj;
//    }

//}
//public class PoolMgr : BaseManager<PoolMgr>
//{
//    #region 注意事项
//    //其布局相关功能只在编辑器界面能看到 正式打包出去之后玩家是看不到的
//    //所以为了避免正式游戏之后 缓存池对象频繁建立父子关系造成的性能开销
//    //这里将布局功能做为可控开关的功能
//    #endregion
//    //用字典来作为柜子容器
//    private Dictionary<string, poolData> poolDic = new Dictionary<string, poolData>();//缓存池字典
//    private GameObject root;//用于布局管理的根对象
//    public static bool isOpenLayout = true; //是否开启布局功能
//    private PoolMgr() { }
//    /// <summary>
//    /// 得到缓存池中的对象
//    /// </summary>
//    /// <param name="name">抽屉容器的名字</param>
//    /// <returns></returns>
//    public GameObject PopObj(string name)
//    {
//        GameObject obj;
//        if (poolDic.ContainsKey(name) && poolDic[name].Count > 0)
//        {
//            obj = poolDic[name].Pop();
//        }
//        else
//        {
//            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));//这里可以添加路径
//            obj.name = name;//确保名字和抽屉容器的名字一致
//        }
//        return obj;
//    }
//    /// <summary>
//    /// 将对象放入缓存池中
//    /// </summary>
//    /// <param name="objname">抽屉容器的名字</param>
//    /// <param name="obj">将要放入的对象</param>
//    public void PushObj(GameObject obj)
//    {
//        if (root == null && isOpenLayout)
//            root = new GameObject("Pool");//创建一个空的根对象 用于存放缓存池中的对象
//        if (!poolDic.ContainsKey(obj.name))
//        {
//            poolDic.Add(obj.name, new poolData(root, obj.name));//如果键存在 抛出异常， 如果键不存在 创建新键
//            //poolDic[name] = new Stack<GameObject>();//如果键存在 值会替换，如果键不存在，创建新键
//        }
//        poolDic[obj.name].Push(obj);
//    }
//    /// <summary>
//    /// 清空缓存池中的所有对象
//    /// </summary>
//    public void Clear()
//    {
//        //切场景时 场景上的对象会被移除 这时应该清空缓存池
//        //否则会出现内存泄漏（内存被占用 且被记录，但实际这些对象都被移除，无法再复用）
//        //并且下次取东西时会出问题
//        poolDic.Clear();
//        if (isOpenLayout)
//            root = null;
//    }
//}

#endregion 未限制数量
public class PoolManager : BaseManager<PoolManager>
{
    #region 注意事项

    //其布局相关功能只在编辑器界面能看到 正式打包出去之后玩家是看不到的
    //所以为了避免正式游戏之后 缓存池对象频繁建立父子关系造成的性能开销
    //这里将布局功能做为可控开关的功能
    #endregion 注意事项

    #region 内部类
    /// <summary>
    /// 单个池容器
    /// </summary>
    private class Pool
    {
        /// <summary>
        /// 池容器中的资源对象
        /// </summary>
        private Stack<PoolBase> _objectStack = new Stack<PoolBase>();

        /// <summary>
        /// 池容器本体
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// 使用中的资源对象列表（取出池容器的）
        /// </summary>
        private List<PoolBase> _usedList = new List<PoolBase>();

        private int _maxCount;
        public int Count => _objectStack.Count;
        public int UsedCount => _usedList.Count;

        /// <summary>
        /// 当前使用中的对象数量是否超过最大数量限制
        /// </summary>
        public bool UsedNotFull => UsedCount < _maxCount;

        /// <summary>
        ///
        /// </summary>
        /// <param name="poolRoot">缓存池根物体（柜子）</param>
        /// <param name="name">抽屉名称</param>
        /// <param name="poolObject">使用的资源对象</param>
        public Pool(GameObject poolRoot, string name, PoolBase poolObject)
        {
            if (PoolManager.IsOpenLayout)
            {
                _root = new GameObject(name);
                _root.transform.SetParent(poolRoot.transform, false);
            }

            //将对象添加到当前激活列表
            AddUsedList(poolObject);

            _maxCount = poolObject.MaxCount;//获取最大数量限制
        }

        public void Push(PoolBase poolObject)
        {
            //如何隐藏该对象
            //方式1：放入缓存池前先将对象设置为不可见
            //方式2：将对象放置在玩家不可见的地方（老版本的Unity频繁激活失活对象会造成一定的性能开销 就使用这种方式）
            if (PoolManager.IsOpenLayout)
                poolObject.transform.SetParent(_root.transform, false);//将对象放入根对象下 方便管理
            poolObject.gameObject.SetActive(false);
            _usedList.Remove(poolObject);//将对象从当前激活列表中移除
            _objectStack.Push(poolObject);//添加对象到抽屉中
        }

        public PoolBase Pop()
        {
            PoolBase poolObject;
            if (Count > 0)
            {
                poolObject = _objectStack.Pop();
                poolObject.gameObject.SetActive(true);
            }
            else
            {
                poolObject = _usedList[0];//将使用中列表时间最长的对象取出来用
                _usedList.RemoveAt(0);//从头部删除 从尾部添加
            }
            AddUsedList(poolObject);//将对象添加到当前激活列表
            if (PoolManager.IsOpenLayout)
                poolObject.transform.SetParent(null, false);//从根对象下移除
            return poolObject;
        }

        public void AddUsedList(PoolBase obj)
        {
            _usedList.Add(obj);//将对象添加到当前激活列表
        }
    }

    /// <summary>
    /// 想要放入缓存池中的数据类和逻辑类 必须继承此接口
    /// </summary>
    public interface IPoolData //接口用于标识数据类和逻辑类等不继承MonoBehaviour的类
    {
        /// <summary>
        /// 该方法是为了清除该数据类对其他对象的引用
        /// 将该类放入缓存池中时 该类应该清空所有引用
        /// 从缓存池中取出复用时 应该重新初始化
        /// </summary>
        void ResetInfo(); //重置数据类或逻辑类的信息
    }

    public abstract class BasePoolData
    { }

    private class PoolData<T> : BasePoolData where T : class, IPoolData, new()
    {
        private Queue<T> _dataQueue = new Queue<T>();

        public T Pop()
        {
            if (_dataQueue.Count > 0)
            {
                return _dataQueue.Dequeue();
            }
            return new T(); //如果队列中没有数据，则创建一个新的对象
        }

        public void Push(T data)
        {
            _dataQueue.Enqueue(data);
        }
    }

    #endregion 内部类

    /// <summary>
    /// 存储继承MonoBehaviour类的字典
    /// 每个值都相当于一个抽屉容器
    /// 键则是抽屉容器的名字
    /// </summary>
    private Dictionary<string, Pool> _poolObjectDict = new Dictionary<string, Pool>();//缓存池字典

    /// <summary>
    /// 存储数据类和逻辑类等不继承MonoBehaviour的字典
    /// </summary>
    private Dictionary<string, BasePoolData> _poolDataDict = new Dictionary<string, BasePoolData>();

    private GameObject _root;//用于布局管理的根对象
    public static bool IsOpenLayout = true; //是否开启布局功能

    private PoolManager()
    { }
    #region GameObject
    /// <summary>
    /// 得到缓存池中的对象（传入poolKey 用Resources加载预制体）
    /// </summary>
    /// <param name="poolKey">池字典的Key 是预制体的名字</param>
    /// <param name="path">相对路径可选</param>
    /// <returns></returns>
    public T Pull<T>(string poolKey, string path = "") where T : PoolBase
    {
        if (_root == null && IsOpenLayout)
            _root = new GameObject("Pool");//创建一个空的根对象 作为所有池容器的根对象

        PoolBase poolObject;
        if (_poolObjectDict.ContainsKey(poolKey) && //必须要有这个池容器
            (_poolObjectDict[poolKey].Count > 0 || //如果池容器中有对象 直接拿
            !_poolObjectDict[poolKey].UsedNotFull))//或者 池容器没有对象了，但是正在使用的对象已经达到上限了，也需要再从使用中的对象里再挤出一个
        {
            poolObject = _poolObjectDict[poolKey].Pop();//从抽屉容器中取出一个对象 在Pop函数内部判断不同情况下应该从抽屉中取出还是直接调用正在激活的对象
            poolObject.OnPull();
        }
        else
        {
            //创建一个新的池对象
            if (!GameObject.Instantiate(Resources.Load<GameObject>(Path.Combine(path, poolKey))).TryGetComponent<PoolBase>(out poolObject))
            {
                Debug.LogError("使用缓存池的对象必须添加PoolBase脚本！");
            }
            //名字和抽屉容器的名字一致 便于区分 外部可以再次改名
            poolObject.gameObject.name = poolKey;
            //设置PoolKey
            poolObject.SetPoolKey(poolKey);

            //如果不存在这个池容器
            if (!_poolObjectDict.ContainsKey(poolKey))
                _poolObjectDict.Add(poolKey, new Pool(_root, poolKey, poolObject));//创建一个新的池容器
            else
                _poolObjectDict[poolKey].AddUsedList(poolObject);//将新对象添加到当前激活列表
        }
        
        poolObject.SetIsInPool(false);
        return poolObject as T;
    }
    /// <summary>
    /// 得到缓存池中的对象（传入预制体 直接加载）
    /// </summary>
    /// <returns></returns>
    public T Pull<T>(GameObject prefab) where T : PoolBase
    {
        string poolKey = prefab.name;

        if (_root == null && IsOpenLayout)
            _root = new GameObject("Pool");//创建一个空的根对象 作为所有池容器的根对象

        PoolBase poolObject;
        if(_poolObjectDict.ContainsKey(poolKey) && //必须要有这个池容器
            (_poolObjectDict[poolKey].Count > 0 || //如果池容器中有对象 直接拿
            !_poolObjectDict[poolKey].UsedNotFull))//或者 池容器没有对象了，但是正在使用的对象已经达到上限了，也需要再从使用中的对象里再挤出一个
        {
            poolObject = _poolObjectDict[poolKey].Pop();//从抽屉容器中取出一个对象 在Pop函数内部判断不同情况下应该从抽屉中取出还是直接调用正在激活的对象
            poolObject.OnPull();
        }
        else
        {
            //创建一个新的池对象
            if (!GameObject.Instantiate(prefab).TryGetComponent<PoolBase>(out poolObject))
            {
                Debug.LogError("使用缓存池的对象必须添加PoolBase脚本！");
            }
            //名字和抽屉容器的名字一致 便于区分 外部可以再次改名
            poolObject.gameObject.name = poolKey;
            //设置PoolKey
            poolObject.SetPoolKey(poolKey);

            //如果不存在这个池容器
            if (!_poolObjectDict.ContainsKey(poolKey))
                _poolObjectDict.Add(poolKey, new Pool(_root, poolKey, poolObject));//创建一个新的池容器
            else
                _poolObjectDict[poolKey].AddUsedList(poolObject);//将新对象添加到当前激活列表
        }

        poolObject.SetIsInPool(false);
        return poolObject as T;
    }
    /// <summary>
    /// 将池对象放入缓存池中（不用的时候手动将其Push回去）
    /// </summary>
    /// <param name="poolObject">将要放入的对象</param>
    public void Push(PoolBase poolObject)
    {
        if (!_poolObjectDict.ContainsKey(poolObject.PoolKey))
            Debug.LogError("没有找到对应的抽屉！");

        poolObject.SetIsInPool(true);
        poolObject.OnPush();
        _poolObjectDict[poolObject.PoolKey].Push(poolObject);
    }
    #endregion

    #region Data
    /// <summary>
    /// 获取自定义数据结构类和逻辑类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nameSpace">可选参数 用于解决不同命名空间下同名类的问题</param>
    /// <returns></returns>
    public T PullData<T>(string nameSpace = null) where T : class, IPoolData, new()
    {
        string poolName = nameSpace + "_" + typeof(T).Name; //获取类型名称作为键
        T result;
        if (!_poolDataDict.ContainsKey(poolName))
        {
            result = new T();
            _poolDataDict.Add(poolName, new PoolData<T>()); //如果缓存池中没有这个数据类或逻辑类，则创建一个新的
        }
        else
        {
            PoolData<T> poolData = _poolDataDict[poolName] as PoolData<T>;
            result = poolData.Pop(); //从缓存池中取出一个或者new一个对象
        }
        return result;
    }



    /// <summary>
    /// 将自定义数据结构类和逻辑类对象压入缓存池中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void PushData<T>(T data, string nameSpace = null) where T : class, IPoolData, new()
    {
        if (data == null) return;
        string poolName = nameSpace + "_" + typeof(T).Name; //获取类型名称作为键
        PoolData<T> poolData;
        if (!_poolDataDict.ContainsKey(poolName))
        {
            poolData = new PoolData<T>();
            //不论是Push还是Pop 如果没有抽屉就new一个抽屉
            _poolDataDict.Add(poolName, poolData);
        }
        else
            poolData = (_poolDataDict[poolName] as PoolData<T>);
        data.ResetInfo();
        poolData.Push(data);
    }
    #endregion
    /// <summary>
    /// 清空缓存池中的所有对象
    /// </summary>
    public void Clear()
    {
        //切场景时 场景上的对象会被移除 这时应该清空缓存池
        //否则会出现内存泄漏（内存被占用 且被记录，但实际这些对象都被移除，无法再复用）
        //并且下次取东西时会出问题
        _poolObjectDict.Clear();
        _root = null;
        _poolDataDict.Clear();
    }
}
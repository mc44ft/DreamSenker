using UnityEngine;

/// <summary>
/// 该脚本挂载在需要调用缓存池的预制体对象上 用于配置数据
/// 所有池预制体中必须挂载一个继承自该基类的组件
/// 该组件可以通过写一个ResetInfo方法用于复用预制体时重置数据
/// </summary>
public abstract class PoolBase : MonoBehaviour
{
    //脚本挂载到预设体上后，在Inspector窗口中配置
    [field: SerializeField] public int MaxCount {  get; private set; }
    
    /// <summary>
    /// 池名称 不再改Hierarchy窗口上的名称了 而是改这个值（在PoolManager中设置）
    /// </summary>
    public string PoolKey {  get; private set; }
    /// <summary>
    /// 该池对象是否在池内（在PoolManager中设置）
    /// </summary>
    public bool IsInPool { get; private set; }
    /// <summary>
    /// 提供给PoolManager设置
    /// </summary>
    /// <param name="poolKey"></param>
    public void SetPoolKey(string poolKey)
    {
        PoolKey = poolKey;
    }
    /// <summary>
    /// 这个方法提供给PoolManager使用
    /// </summary>
    /// <param name="isInPool"></param>
    public void SetIsInPool(bool isInPool)
    {
        IsInPool = isInPool;
    }
    /// <summary>
    /// 该方法在PoolManager中Pull对象时调用（第一次取出时不会调用） 用于重置数据
    /// </summary>
    public abstract void OnPull();
    /// <summary>
    /// 该方法在PoolManager中Push对象时调用 用于恢复数据
    /// </summary>
    public abstract void OnPush();
    /// <summary>
    /// 提供给子类简单重置Transform信息的方法
    /// </summary>
    public void ResetTransformInfo(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
    public void SetParent(Transform parent, bool worldPositionStays = false)
    {
        transform.SetParent(parent, worldPositionStays);
    }
    /// <summary>
    /// 提供给外部和子类简单回收自己的方法 可以重写（base方法记得放最后）
    /// </summary>
    public virtual void PushSelfToPool()
    {
        if (!IsInPool)
        {
            PoolManager.Instance.Push(this);
        }
    }
}
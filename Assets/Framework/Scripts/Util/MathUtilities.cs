using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MathUtilities
{
    #region 角度弧度互转

    /// <summary>
    /// 角度转弧度
    /// </summary>
    /// <param name="deg">角度值</param>
    /// <returns></returns>
    public static float Deg2Rad(float deg)
    {
        return Mathf.Deg2Rad * deg;
    }

    /// <summary>
    /// 弧度转角度
    /// </summary>
    /// <param name="rad">弧度值</param>
    /// <returns></returns>
    public static float Rad2Deg(float rad)
    {
        return Mathf.Rad2Deg * rad;
    }

    #endregion 角度弧度互转

    #region 距离

    /// <summary>
    /// 获取XY平面两点之间的距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float GetDistanceXY(Vector3 a, Vector3 b)
    {
        //结构体修改值 外部不会改变
        a.z = 0;
        b.z = 0;
        return Vector3.Distance(a, b);
    }

    /// <summary>
    /// 检查XY平面两点之间的距离是否小于指定值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static bool CheckDistanceXY(Vector3 a, Vector3 b, float distance)
    {
        return GetDistanceXY(a, b) < distance;
    }

    /// <summary>
    /// 获取XZ平面两点之间的距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float GetDistanceXZ(Vector3 a, Vector3 b)
    {
        a.y = 0;
        b.y = 0;
        return Vector3.Distance(a, b);
    }

    /// <summary>
    /// 检查XZ平面两点之间的距离是否小于指定值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static bool CheckDistanceXZ(Vector3 a, Vector3 b, float distance)
    {
        return GetDistanceXZ(a, b) < distance;
    }

    #endregion 距离

    #region 范围检查

    /// <summary>
    /// 检查指定位置是否在屏幕内
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool CheckScreenInside(Vector3 pos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        return screenPos.x >= 0 && screenPos.x <= Screen.width &&
               screenPos.y >= 0 && screenPos.y <= Screen.height;
    }

    /// <summary>
    /// 判断是否在XYZ平面内的扇形区域内（需位于同一个坐标系下）
    /// </summary>
    /// <param name="selfPos">自身位置</param>
    /// <param name="selfForward">自身面朝向</param>
    /// <param name="targetPos">目标位置</param>
    /// <param name="radius">扇形半径</param>
    /// <param name="angle">扇形角度</param>
    /// <returns></returns>
    public static bool CheckSectorInsideXZ(Vector3 selfPos, Vector3 selfForward, Vector3 targetPos, float radius, float angle)
    {
        selfPos.y = 0;
        targetPos.y = 0;
        return Vector3.Distance(selfPos, targetPos) <= radius &&
               Vector3.Angle(selfForward, targetPos - selfPos) <= angle / 2;
    }

    /// <summary>
    /// 盒形检测
    /// </summary>
    /// <typeparam name="T">填写Collider GameObject 或对象上挂载的脚本</typeparam>
    /// <param name="center">盒子中心点</param>
    /// <param name="halfExtents">盒子长宽高的一半</param>
    /// <param name="rotation">盒子旋转方向</param>
    /// <param name="layerMask">层级遮罩</param>
    /// <param name="callback">回调函数</param>
    /// <param name="interaction">是否忽略触发器 不填默认为全局设置</param>
    public static void OverlapBox<T>(Vector3 center, Vector3 halfExtents, Quaternion rotation, int layerMask,
        UnityAction<T> callback, QueryTriggerInteraction interaction = QueryTriggerInteraction.UseGlobal) where T : class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask, interaction);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callback?.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callback?.Invoke(colliders[i].gameObject as T);
            else
                callback?.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }

    /// <summary>
    /// 球形检测
    /// </summary>
    /// <typeparam name="T">填写Collider GameObject 或对象上挂载的脚本</typeparam>
    /// <param name="center">球中心点</param>
    /// <param name="radius">球半径</param>
    /// <param name="layerMask">层级遮罩</param>
    /// <param name="callback">回调函数</param>
    /// <param name="interaction">是否忽略触发器 不填默认为全局设置</param>
    public static void OverlapSphere<T>(Vector3 center, float radius, int layerMask,
        UnityAction<T> callback, QueryTriggerInteraction interaction = QueryTriggerInteraction.UseGlobal) where T : class
    {
        Type type = typeof(T);
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask, interaction);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (type == typeof(Collider))
                callback?.Invoke(colliders[i] as T);
            else if (type == typeof(GameObject))
                callback?.Invoke(colliders[i].gameObject as T);
            else
                callback?.Invoke(colliders[i].gameObject.GetComponent<T>());
        }
    }

    #endregion 范围检查

    #region 射线检测

    /// <summary>
    /// 射线单体检测（获取RaycastHit信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void Raycast(Ray ray, UnityAction<RaycastHit> callback, int layerMask, float maxDistance = 1000)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callback?.Invoke(hitInfo);
    }

    /// <summary>
    /// 射线单体检测（获取GameObject信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void Raycast(Ray ray, UnityAction<GameObject> callback, int layerMask, float maxDistance = 1000)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callback?.Invoke(hitInfo.collider.gameObject);
    }

    /// <summary>
    /// 射线单体检测（获取指定脚本信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void Raycast<T>(Ray ray, UnityAction<T> callback, int layerMask, float maxDistance = 1000) where T : MonoBehaviour
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
            callback?.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
    }

    /// <summary>
    /// 射线群体检测（获取RaycastHit信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void RaycastAll(Ray ray, UnityAction<RaycastHit> callback, int layerMask, float maxDistance = 1000)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (RaycastHit hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo);
        }
    }

    /// <summary>
    /// 射线群体检测（获取GameObject信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void RaycastAll(Ray ray, UnityAction<GameObject> callback, int layerMask, float maxDistance = 1000)
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (RaycastHit hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo.collider.gameObject);
        }
    }

    /// <summary>
    /// 射线群体检测（获取指定脚本信息）
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="callback">回调函数</param>
    /// <param name="layerMask">遮罩层级</param>
    /// <param name="maxDistance">最大距离</param>
    public static void RaycastAll<T>(Ray ray, UnityAction<T> callback, int layerMask, float maxDistance = 1000) where T : MonoBehaviour
    {
        RaycastHit[] hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (RaycastHit hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
        }
    }

    #endregion 射线检测
    
}
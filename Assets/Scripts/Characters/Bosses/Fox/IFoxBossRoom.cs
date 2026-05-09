using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 使用了依赖倒置原则、
/// 即Boss房间和Boss互相持有引用 但Boss只持有房间的该接口 Boss只能操作该接口 不能完全操作房间
/// </summary>

namespace DreamSenker.Characters.Bosses
{
public interface IFoxBossRoom
{
    /// <summary>
    /// 获取距离目标位置最近的现身点位
    /// </summary>
    /// <returns></returns>
    Vector3 GetMinDistanceFromShowPosition(Vector3 targetPosition);
    /// <summary>
    /// 获取距离目标位置最远的现身点位
    /// </summary>
    /// <returns></returns>
    Vector3 GetMaxDistanceFromShowPosition(Vector3 targetPosition);
    float GetGroundY();
    Transform[] GetClonePointArray();
    Transform GetCloneCenterPoint();
}
}

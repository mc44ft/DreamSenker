
using UnityEngine;
/// <summary>
/// 所有可以被攻击的对象都继承该接口
/// 如果是场景中可以被破坏的物体 继承该接口 被攻击直接被破坏即可
/// </summary>

namespace DreamSeeker.Combat.Health
{
public interface IDamageable
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="attackDirection">击飞方向 与子弹\攻击方向一致</param>
    void TakeDamage(int damage, Vector2 attackDirection);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>自己所在的世界位置</returns>
    Vector2 GetPosition();
    /// <summary>
    /// 被玩家镜像攻击选中时执行逻辑
    /// </summary>
    void Select();
    /// <summary>
    /// 被玩家镜像攻击取消选中时执行逻辑
    /// </summary>
    void Deselect();
}
}

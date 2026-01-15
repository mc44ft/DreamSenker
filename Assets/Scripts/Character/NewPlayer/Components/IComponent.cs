using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// IComponent的最大父接口
/// IComponent-IConfig- 和 IComponent-IMoveConfig-  没有任何关系
/// </summary>
public interface IComponent
{
    public void InjectionConfigBase(IConfig config);
}
public interface IComponent<in T> : IComponent where T : IConfig
{
    /// <summary>
    /// 在玩家切换形态的时候 可更新配置文件
    /// </summary>
    public void InjectionConfig(T config);
}

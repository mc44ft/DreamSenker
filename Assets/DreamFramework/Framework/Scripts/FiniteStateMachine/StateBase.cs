

using UnityEngine;
/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Controller</typeparam>
public abstract class StateBase<T> where T : MonoBehaviour
{
    protected T _controller;//持有控制组件引用
    protected MachineManager<T> _machineManager;//持有状态机的引用 方便切换状态
    public StateBase(T controller, MachineManager<T> machineManager)
    {
        _controller = controller;
        _machineManager = machineManager;
    }
    public abstract void Enter();
    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void Exit();
}

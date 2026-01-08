using PlayArk.GraphCore.Data;

public class State : GraphCoreNode<GraphCorePort>
{
    /// <summary>
    /// 用于提示该状态是否已启动
    /// </summary>
    protected bool _started = false;
    protected StateMachineController _controller;

    public void Bind(StateMachineController stateMachineController)
    {
        _controller = stateMachineController;
    }

    protected virtual void Enter()
    {
        _started = true;
    }

    protected virtual void LogicUpdate()
    {
        //在这里进行状态轮询的条件判断
        
    }

    protected virtual void PhysicsUpdate()
    {
        
    }

    protected virtual void Exit()
    {
        _started = false;
    }
}

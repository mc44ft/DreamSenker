
using System.Collections;
using UnityEngine;

/// <summary>
/// 技能释放结束后 回到Hide状态
/// </summary>

namespace DreamSenker.Characters.Bosses
{
public class FoxTwoHideState : StateBase<FoxTwoController>
{
    public FoxTwoHideState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
        
    }

    public override void Enter()
    {
        _controller.StartCoroutine(HidingRoutine());
    }
    
    public override void Exit()
    {
        
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
    private IEnumerator HidingRoutine()
    {
        //开始隐匿
        yield return _controller.StartHide();

        //等待设置的最小时长
        yield return new WaitForSeconds(_controller.FoxTwoConfig.HideMinTime);

        StateBase<FoxTwoController> skillState;
        //等待可用技能
        while (!_controller.TryGetCooldownFinishedSkill(out skillState))
        {
            yield return skillState;
        }
        //切换到对应的技能状态
        _machineManager.TransitionTo(skillState);
    }
}
}

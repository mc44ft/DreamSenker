using UnityEngine;


namespace DreamSeeker.Characters.Bosses
{
public class SpiderIdleState : StateBase<SpiderController>
{
    public SpiderIdleState(SpiderController controller, MachineManager<SpiderController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        //确定下一次的目标点位
        Waypoint targetWaypoint = _controller.Waypoints[Random.Range(0, _controller.Waypoints.Count)];
        while (targetWaypoint == _controller.CurrentWaypoint)
        {
            targetWaypoint = _controller.Waypoints[Random.Range(0, _controller.Waypoints.Count)];
        }
        _controller.TargetWaypoint = targetWaypoint;

        _controller.CooldownTimer = _controller.SpiderConfig.IdleTime;
    }

    public override void Exit()
    {
        
    }

    public override void LogicUpdate()
    {
        if (_controller.CooldownTimer < 0f)
        {
            //判断技能冷却
            if (_controller.SkillHealCooldownTimer <= 0f && _controller.HealthAmount < 50)//血量要求
            {

                _machineManager.TransitionTo(_controller.SpiderSkillHealState);
            }
            else if (_controller.SkillVenomBiteCooldownTimer <= 0f)//不设置幽灵状态 而是在此处设置参数
            {

                //更新追击速度
                _controller.MoveSpeed = _controller.SpiderConfig.ChaseSpeed;
                //更新目标位置
                _controller.TargetWaypoint = _controller.GetNearestWaypoint(_controller.Player.position);

                _machineManager.TransitionTo(_controller.SpiderCrawlState);
            }
            else if (_controller.SkillWebCooldownTimer <= 0f)
            {

                _machineManager.TransitionTo(_controller.SpiderSkillWebState);
            }
            else if (_controller.SkillhakeCooldownTimer <= 0f)
            {

                //更新追击速度
                _controller.MoveSpeed = _controller.SpiderConfig.ReturnCenterSpeed;
                //更新目标位置
                _controller.TargetWaypoint = _controller.WebCenterWaypoint;

                _machineManager.TransitionTo(_controller.SpiderCrawlState);
            }

            else
            {
                //设定速度
                _controller.MoveSpeed = _controller.SpiderConfig.CrawlSpeed;

                _machineManager.TransitionTo(_controller.SpiderCrawlState);
            }
        }
        _controller.CooldownTimer -= Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        
    }
}
}

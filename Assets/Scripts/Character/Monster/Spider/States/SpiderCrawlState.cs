using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpiderController;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpiderCrawlState : StateBase<SpiderController>
{
    private int _nowPathIndex;
    public SpiderCrawlState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {
        //播放爬行动画
        _controller.Animator.Play(Settings.SpiderAnimNameToHash_Crawl);

        if (_controller.CurrentWaypoint != null)
            _controller.NowPath = _controller.FindPathAstar(_controller.CurrentWaypoint, _controller.TargetWaypoint);

        //倒叙遍历路径节点 将当前航点索引设置为起点
        _nowPathIndex = _controller.NowPath.Count - 1;
    }

    public override void Exit()
    {
        //清空航线
        _controller.NowPath.Clear();
        _controller.NowPath = null;

        //清空当前目标点位
        _controller.TargetWaypoint = null;
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        if (_nowPathIndex < 0)
        {
            //切换状态
            if (_controller.MoveSpeed == _controller.SpiderConfig.ChaseSpeed)//追击状态下转换为Skill_2
            {
                _machineManager.TransitionTo(_controller.SpiderSkillVenomBiteState);
            }
            else if (_controller.MoveSpeed == _controller.SpiderConfig.ReturnCenterSpeed)
            {
                _machineManager.TransitionTo(_controller.SpiderSkillShakeState);
            }
            else
            {
                _machineManager.TransitionTo(_controller.SpiderIdleState);
            }

            return;
        }
        if (_controller.CurrentWaypoint != null && _controller.CurrentWaypoint == _controller.NowPath[_nowPathIndex])//抵达当前航点
        {
            //更新到下一个航点的索引
            _nowPathIndex--;
        }
        else
        {
            //更新转向
            _controller.UpdateFace((_controller.NowPath[_nowPathIndex].transform.position.x - _controller.transform.position.x) < 0 ? 1 : -1);
            //爬行到下一个航点
            Vector2 unitVector = (_controller.NowPath[_nowPathIndex].transform.position - _controller.transform.position).normalized;
            _controller.Rigidbody.MovePosition(_controller.Rigidbody.position + unitVector * _controller.MoveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(_controller.transform.position, _controller.NowPath[_nowPathIndex].transform.position) < 0.2f)
            {
                _controller.CurrentWaypoint = _controller.NowPath[_nowPathIndex];//更新当前所在航点
            }
        }
    }
}

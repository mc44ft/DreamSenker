using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    public enum EPredicate
    {
        KeyCodePressed,//按下按键
        AnimOver,//动画播放结束
        HorizontalNotZero,//水平输入不为0
        VerticalSpeedNotNegative,//垂直速度不为负数
        JumpCountNotZero,//跳跃次数不为0
        Grounded, //接地状态
        TakeDamage,//受到攻击
        Dead,//触发死亡
        AttackCooldownReady,//攻击冷却结束
        //--------------- OldMan AI -----------
        OldManCanSeePlayer,//OldMan发现玩家
        OldManInAttackRange,//玩家进入OldMan攻击范围
        OldManLostPlayer,//OldMan丢失玩家
    }
}
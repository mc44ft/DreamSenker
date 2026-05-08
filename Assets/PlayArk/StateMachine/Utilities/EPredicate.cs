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
        //--------------- AI -----------
        CanSeeTarget,//发现目标
        InAttackRange,//目标进入攻击范围
        LostTarget,//丢失目标
    }
}
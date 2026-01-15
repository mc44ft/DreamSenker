using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(AnimationPlayer))]
public class Attacker : BaseComponent<IAttackConfig>
{
    private bool _isAttacked;
    private IAttackConfig _attackConfig;
    private AnimationPlayer _animationPlayer;

    private void Awake()
    {
        _animationPlayer = GetComponent<AnimationPlayer>();
    }
    
    public override void InjectionConfig(IAttackConfig config)
    {
        _attackConfig = config;
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        return null;
    }
}

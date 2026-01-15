using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

public class Flyer : BaseComponent<IConfig>
{

    public override void InjectionConfig(IConfig moveConfig)
    {
        
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        return null;
    }
}

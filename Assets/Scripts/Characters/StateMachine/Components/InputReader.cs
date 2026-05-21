using PlayArk.StateMachine.Utilities;
using UnityEngine;

using DreamSeeker.Managers;

using DreamSeeker.Characters;

namespace DreamSeeker.Characters.Player
{
public class InputReader : BaseComponent<IConfig>
{
    public bool CheckKeyCodePressed(string keyName)
    {
        if (keyName == "SwitchFormButtonDown")
        {
            return InputManager.Instance.SwitchFormButtonDown;
        }
        if (keyName == "JumpButtonDown")
        {
            return InputManager.Instance.JumpButtonDown;
        }
        if (keyName == "AttackButtonDown")
        {
            return InputManager.Instance.AttackButtonDown;
        }
        return false;
    }

    private bool CheckHorizontalNotZero()
    {
        return InputManager.Instance.HorizontalValue != 0;
    }

    public override void InjectionConfig(IConfig moveConfig)
    {
        
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.KeyCodePressed:
                return CheckKeyCodePressed(parameters[0]);
            case EPredicate.HorizontalNotZero:
                return CheckHorizontalNotZero();
        }

        return null;
    }
}
}


using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class AnimationPlayer : BaseComponent<IConfig>
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetRuntimeAnimatorController(RuntimeAnimatorController controller)
    {
        _animator.runtimeAnimatorController = controller;
    }
    public void PlayAnimation(string animName)
    {
        _animator.Play(animName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="animTag">每个动作可能对应多个不同的动画
    /// 只要给相同类型的动画状态打上标签，就可以实现动画分组
    /// 检测该组动画是否播放结束即可</param>
    private bool CheckAnimOver(string animTag)
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        
        //如果有混合动画 第二个判断条件起作用
        if (info.IsTag(animTag) && !_animator.IsInTransition(0))
        {
            return info.normalizedTime >= 1;
        }
        return false;
    }

    public override void InjectionConfig(IConfig moveConfig)
    {
        
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.AnimOver:
                //检测Tag分组动画是否播放完毕
                return CheckAnimOver(parameters[0]);
        }
        return null;
    }
}

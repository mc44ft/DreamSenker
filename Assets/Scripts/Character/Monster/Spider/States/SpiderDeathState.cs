using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderDeathState : StateBase<SpiderController>
{
    public SpiderDeathState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {
        Debug.Log("死亡Enter");
        AudioManager.Instance.PlaySound(GameResources.Instance.SpiderDeathClip);

        _controller.SpriteRenderer.color = Color.gray;
        _controller.transform.localScale = new Vector3(_controller.transform.localScale.x, -1, 1);
        _controller.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _controller.Rigidbody.gravityScale = 5;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIdleState : HandState
{
    public HandIdleState(Hand hand, HandStateMachine stateMachine) : base(hand, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 进入闲置状态时释放物体（如果有的话）
        // Release();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if (hand.handpose == HandPose.Fist)
        {
            stateMachine.ChangeState(hand.grabState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private void Release()
    {
        var held = hand.currentHeldObject;
        if (held != null)
        {
            held.OnRelease(hand);
            held.transform.SetParent(null);

            var rb = held.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;

            hand.currentHeldObject = null;
        }

        rig.enabled = true;

        // 停止使用抓握姿势，回到MediaPipe控制
        if (rig != null)
        {
            rig.StopGrabPose();
        }
    }
}
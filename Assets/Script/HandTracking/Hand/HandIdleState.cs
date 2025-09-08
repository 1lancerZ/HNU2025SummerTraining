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
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(hand.handpose == HandPose.Fist)
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
    }
}

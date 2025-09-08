using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabState : HandState
{
    public HandGrabState(Hand hand, HandStateMachine stateMachine) : base(hand, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();


        Collider[] colliders = Physics.OverlapSphere(hand.grabAnchor.position, 0.3f);
        foreach (var col in colliders)
        {
            var gun = col.GetComponent<Gun>();
            if (gun != null)
            {
                Grab(gun);
                break;
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(hand.handpose == HandPose.Palm)
        {
            stateMachine.ChangeState(hand.idleState);
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

    private void Grab(Gun target)
    {
        hand.currentHeldObject = target;

        target.transform.SetParent(hand.grabAnchor);
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;

        var rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        rig.enabled = false;

        //ApplyGrabPose();

        // 调用对象的 OnGrab 回调
        target.OnGrab(hand);
    }
}
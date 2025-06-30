using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFiringState : GunState
{
    public GunFiringState(Gun gun, GunStateMachine stateMachine, string animBoolName) : base(gun, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        gun.Fire();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (triggerCalled)
        {
            gun.StateMachine.ChangeState(gun.idleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}

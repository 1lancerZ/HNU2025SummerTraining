using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunIdleState : GunState
{
    public GunIdleState(Gun gun, GunStateMachine stateMachine, string animBoolName) : base(gun, stateMachine, animBoolName)
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
        if(Input.GetMouseButtonDown(0) && gun.currentAmmo > 0)
        {
            gun.StateMachine.ChangeState(gun.firingState);
        }
        else if (gun.currentAmmo <= 0)
        {
            gun.StateMachine.ChangeState(gun.reloadState);
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
}

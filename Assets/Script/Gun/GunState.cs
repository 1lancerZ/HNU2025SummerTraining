using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunState
{
    protected GunStateMachine stateMachine;
    protected Gun gun;
    protected float stateTimer;
    protected bool triggerCalled;

    private string animBoolName;

    public GunState(Gun gun, GunStateMachine stateMachine, string animBoolName)
    {
        this.gun = gun;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter() {
        stateTimer = 0f;
        gun.anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }
    public virtual void Exit() {
        gun.anim.SetBool(animBoolName, false);
    }
    public virtual void HandleInput() { }
    public virtual void LogicUpdate()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void PhysicsUpdate() {
    
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}

using landmarktest;
using UnityEngine;

public abstract class HandState
{
    protected Hand hand;
    protected HandStateMachine stateMachine;
    protected HandRigController rig => hand.rigController;

    protected float stateTimer;
    protected bool triggerCalled;

    public HandState(Hand hand, HandStateMachine stateMachine)
    {
        this.hand = hand;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        stateTimer = 0f;
        triggerCalled = false;
    }

    public virtual void Exit() { }

    public virtual void HandleInput() { }

    public virtual void LogicUpdate()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void PhysicsUpdate() { }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }

    public HandSide Side => hand.handSide;
}

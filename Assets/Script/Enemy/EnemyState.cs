using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;
    protected float stateTimer;
    protected bool triggerCalled;

    private string animBoolName;

    public EnemyState(Enemy enemy, EnemyStateMachine stateMachine, string animBoolName)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        stateTimer = 0f;
        enemy.anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }
    public virtual void Exit()
    {
        enemy.anim.SetBool(animBoolName, false);
    }
    public virtual void HandleInput() { }
    public virtual void LogicUpdate()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void PhysicsUpdate()
    {

    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        //Debug.Log($"Changing state to: {newState.GetType().Name}");
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.HandleInput();
        CurrentState?.LogicUpdate();
    }

    public void FixedUpdate()
    {
        CurrentState?.PhysicsUpdate();
    }
}

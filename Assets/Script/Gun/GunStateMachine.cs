using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunStateMachine
{
    public GunState CurrentState { get; private set; }

    public void Initialize(GunState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(GunState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
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

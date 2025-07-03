public class HandStateMachine
{
    public HandState CurrentState { get; private set; }

    public void Initialize(HandState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(HandState newState)
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

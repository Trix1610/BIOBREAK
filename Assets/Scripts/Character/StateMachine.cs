using System;

public class StateMachine
{
    private ICharacterState currentState;

    public ICharacterState CurrentState => currentState;

    public void ChangeState(ICharacterState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }
}

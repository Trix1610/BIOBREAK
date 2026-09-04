using UnityEngine;

public interface ICharacterState
{
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
}

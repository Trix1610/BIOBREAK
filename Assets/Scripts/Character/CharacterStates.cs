using UnityEngine;

public class IdleState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;

    public IdleState(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
    }

    public void Enter()
    {
        // Анимация Idle
    }

    public void Exit()
    {
    }

    public void Update()
    {
        if (controller.MoveInput.magnitude > 0.1f)
        {
            controller.StateMachine.ChangeState(new RunState(controller, stats));
        }
    }

    public void FixedUpdate()
    {
        controller.StopHorizontalMovement();
    }
}

public class RunState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;

    public RunState(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
    }

    public void Enter()
    {
        // Анимация Run
    }

    public void Exit()
    {
    }

    public void Update()
    {
        if (controller.MoveInput.magnitude < 0.1f)
        {
            controller.StateMachine.ChangeState(new IdleState(controller, stats));
        }

        if (!controller.IsGrounded)
        {
            controller.StateMachine.ChangeState(new FallState(controller, stats));
        }
    }

    public void FixedUpdate()
    {
        controller.HandleMovement();
    }
}

public class JumpState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;

    public JumpState(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
    }

    public void Enter()
    {
        controller.HandleJump();
        // Анимация Jump
    }

    public void Exit()
    {
    }

    public void Update()
    {
        if (controller.IsGrounded)
        {
            controller.StateMachine.ChangeState(new IdleState(controller, stats));
        }
        else if (controller.Rigidbody.linearVelocity.y < 0)
        {
            controller.StateMachine.ChangeState(new FallState(controller, stats));
        }
    }

    public void FixedUpdate()
    {
        controller.HandleMovement();
    }
}

public class FallState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;

    public FallState(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
    }

    public void Enter()
    {
        // Анимация Fall
    }

    public void Exit()
    {
    }

    public void Update()
    {
        if (controller.IsGrounded)
        {
            controller.StateMachine.ChangeState(new IdleState(controller, stats));
        }
    }

    public void FixedUpdate()
    {
        controller.HandleMovement();
    }
}

public class HurtState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;
    private readonly float hurtDuration;

    private float hurtTimer;

    public HurtState(CharacterController controller, CharacterStats stats, float hurtDuration = 0.5f)
    {
        this.controller = controller;
        this.stats = stats;
        this.hurtDuration = hurtDuration;
    }

    public void Enter()
    {
        hurtTimer = hurtDuration;
        // Анимация Hurt, knockback
    }

    public void Exit()
    {
    }

    public void Update()
    {
        hurtTimer -= Time.deltaTime;

        if (hurtTimer <= 0)
        {
            if (controller.IsGrounded)
            {
                controller.StateMachine.ChangeState(new IdleState(controller, stats));
            }
            else
            {
                controller.StateMachine.ChangeState(new FallState(controller, stats));
            }
        }
    }

    public void FixedUpdate()
    {
        // Можно добавить knockback
    }
}

public class DeathState : ICharacterState
{
    private readonly CharacterController controller;
    private readonly CharacterStats stats;

    public DeathState(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
    }

    public void Enter()
    {
        // Анимация Death, отключить управление
        controller.enabled = false;
    }

    public void Exit()
    {
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
    }
}

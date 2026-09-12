using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class CharacterMovement
    {
        private readonly CharacterMovementConfig config;
        private readonly CharacterGroundCheck groundCheck;

        public CharacterMovement(CharacterMovementConfig config, CharacterGroundCheck groundCheck)
        {
            this.config = config;
            this.groundCheck = groundCheck;
        }

        public void HandleMovement(Vector2 moveInput, bool isInputLocked)
        {
            float targetX = isInputLocked ? 0f : moveInput.x * config.Stats.MoveSpeed;

            config.Rigidbody.linearVelocity = new Vector2(
                targetX,
                config.Rigidbody.linearVelocity.y
            );
        }

        public void StopHorizontalMovement()
        {
            config.Rigidbody.linearVelocity = new Vector2(
                0,
                config.Rigidbody.linearVelocity.y
            );
        }

        public void HandleJump()
        {
            config.Rigidbody.linearVelocity = new Vector2(
                config.Rigidbody.linearVelocity.x,
                config.Stats.JumpForce
            );
        }

        public void HandleJumpRelease()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                if (config.Rigidbody.linearVelocity.y > 0)
                {
                    config.Rigidbody.linearVelocity = new Vector2(
                        config.Rigidbody.linearVelocity.x,
                        config.Rigidbody.linearVelocity.y * 0.5f
                    );
                }
            }
        }

        public bool CanJump(int currentJumps, int maxJumps)
        {
            return (groundCheck.IsGrounded && maxJumps > 0) || currentJumps > 0;
        }
    }
}

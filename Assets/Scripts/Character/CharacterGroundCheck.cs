using UnityEngine;

namespace Character
{
    public class CharacterGroundCheck
    {
        private readonly CharacterGroundCheckConfig config;
        private bool wasGrounded;

        public bool IsGrounded { get; private set; }

        public CharacterGroundCheck(CharacterGroundCheckConfig config)
        {
            this.config = config;
        }

        public void Update()
        {
            IsGrounded = config.GroundCheck != null &&
                         Physics2D.OverlapCircle(
                             config.GroundCheck.position,
                             config.GroundCheckRadius,
                             config.GroundLayer
                         );
        }

        public int CheckLanding(int currentJumps, int maxJumps)
        {
            if (IsGrounded && !wasGrounded)
            {
                currentJumps = maxJumps;
            }

            wasGrounded = IsGrounded;
            return currentJumps;
        }
    }
}

using UnityEngine;

namespace Enemies
{
    public class EnemyAI
    {
        private readonly Transform transform;
        private readonly Rigidbody2D rb;
        private readonly Transform playerTransform;
        private readonly EnemyMovementConfig movementConfig;
        private readonly EnemyJumpConfig jumpConfig;
        private readonly EnemyRaycastConfig raycastConfig;

        private bool _isGrounded;
        private bool _platformAbove;
        private int _currentMoveDirection = 1;
        private bool _wasUnderPlatform;
        private bool _isWalkingPastEdge;
        private float _edgePositionX;
        private int _exitDirection = 1;
        private float _nextJumpTime;

        public EnemyAI(
            Transform transform,
            Rigidbody2D rb,
            Transform playerTransform,
            EnemyMovementConfig movementConfig,
            EnemyJumpConfig jumpConfig,
            EnemyRaycastConfig raycastConfig)
        {
            this.transform = transform;
            this.rb = rb;
            this.playerTransform = playerTransform;
            this.movementConfig = movementConfig;
            this.jumpConfig = jumpConfig;
            this.raycastConfig = raycastConfig;
        }

        public bool IsGrounded => _isGrounded;

        public void Update()
        {
            if (playerTransform == null) return;

            _isGrounded = raycastConfig.GroundCheck != null && 
                         Physics2D.OverlapCircle(raycastConfig.GroundCheck.position, raycastConfig.GroundCheckRadius, raycastConfig.GroundLayer);

            _platformAbove = raycastConfig.PlatformCheck != null && 
                             Physics2D.Raycast(raycastConfig.PlatformCheck.position, Vector2.up, raycastConfig.PlatformCheckDistance, raycastConfig.GroundLayer).collider != null;

            if (!_isGrounded) return;

            bool playerIsHigher = playerTransform.position.y > transform.position.y + 0.3f;
            float distToPlayerX = Mathf.Abs(playerTransform.position.x - transform.position.x);
            float dirToPlayerX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            bool shouldConsiderJump =
                playerTransform.position.y - transform.position.y >= jumpConfig.MinJumpHeight &&
                distToPlayerX <= jumpConfig.MaxJumpDistanceX &&
                Time.time >= _nextJumpTime;

            if (_platformAbove)
            {
                _wasUnderPlatform = true;
                _isWalkingPastEdge = false;
            }
            else if (_wasUnderPlatform && !_isWalkingPastEdge)
            {
                _isWalkingPastEdge = true;
                _edgePositionX = transform.position.x;
                _exitDirection = _currentMoveDirection;
            }

            if (_isWalkingPastEdge)
            {
                if (Mathf.Abs(transform.position.x - _edgePositionX) >= movementConfig.ExtraStepAfterEdge)
                {
                    if (shouldConsiderJump)
                        TryJump(dirToPlayerX);

                    _isWalkingPastEdge = false;
                    _wasUnderPlatform = false;
                }
            }
            else if (!_platformAbove && shouldConsiderJump)
            {
                Vector2 origin = raycastConfig.DiagCheckPoint != null ? (Vector2)raycastConfig.DiagCheckPoint.position : (Vector2)transform.position;
                Vector2 rayDir = new Vector2(dirToPlayerX, 1.0f).normalized;
                
                Debug.DrawRay(origin, rayDir * raycastConfig.DiagCheckDistance, Color.red);

                if (Physics2D.Raycast(origin, rayDir, raycastConfig.DiagCheckDistance, raycastConfig.GroundLayer).collider != null)
                {
                    TryJump(dirToPlayerX);
                }
            }
        }

        public void FixedUpdate()
        {
            if (playerTransform == null) return;

            float directionX;

            if (_platformAbove)
            {
                directionX = _currentMoveDirection;
            }
            else if (_isWalkingPastEdge)
            {
                directionX = _exitDirection;
            }
            else
            {
                float distToPlayerX = playerTransform.position.x - transform.position.x;
                if (Mathf.Abs(distToPlayerX) > 0.2f)
                    _currentMoveDirection = (int)Mathf.Sign(distToPlayerX);
                
                directionX = _currentMoveDirection;
            }

            rb.linearVelocity = new Vector2(directionX * movementConfig.Speed, rb.linearVelocity.y);

            if (directionX != 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(directionX), transform.localScale.y, transform.localScale.z);
        }

        private void Jump(float dirX)
        {
            rb.linearVelocity = new Vector2(dirX * (movementConfig.Speed * 0.6f), movementConfig.JumpForce);
            _isWalkingPastEdge = false;
            _wasUnderPlatform = false;
        }

        private bool TryJump(float dirX)
        {
            if (Time.time < _nextJumpTime || !_isGrounded)
                return false;

            _nextJumpTime = Time.time + jumpConfig.JumpCooldown;
            Jump(dirX);
            return true;
        }
    }
}

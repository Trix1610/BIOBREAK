using UnityEngine;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float extraStepAfterEdge = 1f;

        [Header("Jump Conditions")]
        [SerializeField] private float maxJumpDistanceX = 6.0f;

        [Header("Raycast Checks")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform platformCheck;
        [SerializeField] private float platformCheckDistance = 2.0f;
        
        [Header("Diagonal Jump Check")]
        [SerializeField] private Transform diagCheckPoint;
        [SerializeField] private float diagCheckDistance = 4.5f;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        private int _currentHealth;

        private Transform _playerTransform;
        private Rigidbody2D _rb;
        private bool _isGrounded;
        
        private int _currentMoveDirection = 1; 
        private bool _platformAbove;

        private bool _wasUnderPlatform = false;
        private bool _isWalkingPastEdge = false;
        private float _edgePositionX;
        private int _exitDirection = 1;

        private void Awake()
        {
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
                Destroy(gameObject);
        }

        private void Start()
        {
            _currentHealth = maxHealth;
            _rb = GetComponent<Rigidbody2D>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            _isGrounded = groundCheck != null && 
                         Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            _platformAbove = platformCheck != null && 
                             Physics2D.Raycast(platformCheck.position, Vector2.up, platformCheckDistance, groundLayer).collider != null;

            if (!_isGrounded) return;

            bool playerIsHigher = _playerTransform.position.y > transform.position.y + 0.3f;
            float distToPlayerX = Mathf.Abs(_playerTransform.position.x - transform.position.x);
            float dirToPlayerX = Mathf.Sign(_playerTransform.position.x - transform.position.x);

            // 1. Логика под платформой и выход за край
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
                if (Mathf.Abs(transform.position.x - _edgePositionX) >= extraStepAfterEdge)
                {
                    if (playerIsHigher && distToPlayerX <= maxJumpDistanceX)
                        Jump(dirToPlayerX);

                    _isWalkingPastEdge = false;
                    _wasUnderPlatform = false;
                }
            }
            // 2. Диагональный прыжок на открытом пространстве
            else if (!_platformAbove && playerIsHigher && distToPlayerX <= maxJumpDistanceX)
            {
                Vector2 origin = diagCheckPoint != null ? (Vector2)diagCheckPoint.position : (Vector2)transform.position;
                Vector2 rayDir = new Vector2(dirToPlayerX, 1.0f).normalized;
                
                Debug.DrawRay(origin, rayDir * diagCheckDistance, Color.red);

                if (Physics2D.Raycast(origin, rayDir, diagCheckDistance, groundLayer).collider != null)
                {
                    Jump(dirToPlayerX);
                }
            }
        }

        private void FixedUpdate()
        {
            if (_playerTransform == null) return;

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
                float distToPlayerX = _playerTransform.position.x - transform.position.x;
                if (Mathf.Abs(distToPlayerX) > 0.2f)
                    _currentMoveDirection = (int)Mathf.Sign(distToPlayerX);
                
                directionX = _currentMoveDirection;
            }

            _rb.linearVelocity = new Vector2(directionX * speed, _rb.linearVelocity.y);

            if (directionX != 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(directionX), transform.localScale.y, transform.localScale.z);
        }

        private void Jump(float dirX)
        {
            _rb.linearVelocity = new Vector2(dirX * (speed * 0.6f), jumpForce);
            _isWalkingPastEdge = false;
            _wasUnderPlatform = false;
        }

        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0) Die();
        }

        private void Die()
        {
            RunManager.Instance?.MarkCurrentRoomAsCleared();
            Destroy(gameObject);
        }
    }
}
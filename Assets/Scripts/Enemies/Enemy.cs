using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Combat Settings")]
        [SerializeField] private int damageAmount = 10;

        [Header("Raycast Checks")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform platformCheck;
        [SerializeField] private float platformCheckDistance = 2.0f;
        
        [Header("Diagonal Jump Check")]
        [SerializeField] private Transform diagCheckPoint;
        [SerializeField] private float diagCheckDistance = 4.5f;

        [Header("Health & UI Settings")]
        [SerializeField] private int maxHealth = 60;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private GameObject healthCanvasObject;
        [SerializeField] private float healthLerpSpeed = 10f;

        [Header("Visual Effects")]
        [SerializeField] private SpriteRenderer spriteRenderer; // Ссылка на спрайт для мигания
        [SerializeField] private float flashDuration = 0.15f;     // Длительность вспышки
        
        private int _currentHealth;
        private float _targetFillAmount;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private Transform _playerTransform;
        private Rigidbody2D _rb;
        private bool _isGrounded;
        
        private int _currentMoveDirection = 1; 
        private bool _platformAbove;

        private bool _wasUnderPlatform;
        private bool _isWalkingPastEdge;
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
            _targetFillAmount = 1f;
            _rb = GetComponent<Rigidbody2D>();

            // Если SpriteRenderer не назначен через инспектор, пытаемся найти его автоматически
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                _originalColor = spriteRenderer.color;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;

            if (healthCanvasObject != null)
                healthCanvasObject.SetActive(false);

            if (healthFillImage != null)
                healthFillImage.fillAmount = 1f;
        }

        private void Update()
        {
            // Безопасная проверка: обновляем полоску здоровья только если канвас существует и активен
            if (healthFillImage != null && healthFillImage.canvas != null)
            {
                healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, _targetFillAmount, Time.deltaTime * healthLerpSpeed);

                Transform canvasTransform = healthFillImage.canvas.transform;
                Vector3 canvasScale = canvasTransform.localScale;
                canvasScale.x = Mathf.Abs(canvasScale.x) * Mathf.Sign(transform.localScale.x);
                canvasTransform.localScale = canvasScale;
            }

            if (_playerTransform == null) return;

            _isGrounded = groundCheck != null && 
                         Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            _platformAbove = platformCheck != null && 
                             Physics2D.Raycast(platformCheck.position, Vector2.up, platformCheckDistance, groundLayer).collider != null;

            if (!_isGrounded) return;

            bool playerIsHigher = _playerTransform.position.y > transform.position.y + 0.3f;
            float distToPlayerX = Mathf.Abs(_playerTransform.position.x - transform.position.x);
            float dirToPlayerX = Mathf.Sign(_playerTransform.position.x - transform.position.x);

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                bool damageDealt = false;

                var scripts = collision.gameObject.GetComponents<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    var method = script.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                    if (method != null)
                    {
                        method.Invoke(script, new object[] { damageAmount });
                        damageDealt = true;
                        Debug.Log($"Враг нанес {damageAmount} урона через скрипт: {script.GetType().Name}");
                        break;
                    }
                }

                if (!damageDealt)
                {
                    Debug.LogWarning("Внимание: Объект с тегом Player столкнулся с врагом, но на нем не найден скрипт с методом TakeDamage(int)!");
                }
            }
        }

        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            Debug.Log($"Враг получил урон: {damage}. Осталось здоровья: {_currentHealth}");
            
            if (healthCanvasObject != null && !healthCanvasObject.activeSelf)
            {
                healthCanvasObject.SetActive(true);
            }

            _targetFillAmount = Mathf.Clamp01((float)_currentHealth / maxHealth);

            // Запускаем эффект мигания белым цветом
            if (spriteRenderer != null)
            {
                if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
                _flashCoroutine = StartCoroutine(FlashWhiteRoutine());
            }

            if (_currentHealth <= 0) Die();
        }

        private IEnumerator FlashWhiteRoutine()
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = _originalColor;
        }

        private void Die()
        {
            // Отключаем коллайдер и физику, чтобы мертвый враг не наносил урон и не падал сквозь стены
            var collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            
            if (_rb != null) 
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.simulated = false;
            }

            // Отключаем этот скрипт движения, чтобы враг больше не двигался
            enabled = false;

            // Запускаем красивую анимацию исчезновения
            StartCoroutine(DeathAnimationRoutine());
        }

        private IEnumerator DeathAnimationRoutine()
        {
            float duration = 0.25f; // Длительность анимации смерти в секундах
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;

            // Скрываем полоску здоровья сразу при смерти
            if (healthCanvasObject != null)
                healthCanvasObject.SetActive(false);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Плавное уменьшение до нуля (схлопывание)
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

                // Плавное уменьшение прозрачности (если есть спрайт)
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnEnable()
        {
            _currentHealth = maxHealth;
            _targetFillAmount = 1f;
            if (healthCanvasObject != null)
                healthCanvasObject.SetActive(false);
        }
    }
}
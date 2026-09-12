using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float extraStepAfterEdge = 1f;

        [Header("Jump Conditions")]
        [SerializeField] private float maxJumpDistanceX = 6.0f;
        [SerializeField] private float minJumpHeight = 0.75f;
        [SerializeField] private float jumpCooldown = 0.8f;

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
        [SerializeField] private SpriteRenderer spriteRenderer; 
        [SerializeField] private float flashDuration = 0.15f;     

        private EnemyAI _ai;
        private EnemyHealth _health;
        private EnemyDeathAnimation _deathAnimation;
        private Transform _playerTransform;
        private Rigidbody2D _rb;
        private Collider2D _collider;

        private void Awake()
        {
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
                Destroy(gameObject);

            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;

            var movementConfig = new EnemyMovementConfig(speed, jumpForce, extraStepAfterEdge);
            var jumpConfig = new EnemyJumpConfig(maxJumpDistanceX, minJumpHeight, jumpCooldown);
            var raycastConfig = new EnemyRaycastConfig(groundCheck, groundCheckRadius, groundLayer, platformCheck, platformCheckDistance, diagCheckPoint, diagCheckDistance);
            var healthConfig = new EnemyHealthConfig(maxHealth, healthFillImage, healthCanvasObject, healthLerpSpeed, spriteRenderer, flashDuration, _rb, transform);
            var deathConfig = new EnemyDeathConfig(transform, spriteRenderer, healthCanvasObject, _rb, _collider);

            _ai = new EnemyAI(transform, _rb, _playerTransform, movementConfig, jumpConfig, raycastConfig);
            _health = new EnemyHealth(healthConfig);
            _deathAnimation = new EnemyDeathAnimation(deathConfig);
        }

        private void Start()
        {
            _health.Initialize();
        }

        private void Update()
        {
            _ai.Update();
            _health.Update();
        }

        private void FixedUpdate()
        {
            _ai.FixedUpdate();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            IDamageable damageable =
                collision.gameObject.GetComponentInParent<IDamageable>();

            DamageSystem.Apply(damageable, damageAmount);
        }

        public float CurrentHealth => _health.CurrentHealth;

        void IDamageable.TakeDamage(float damage)
        {
            TakeDamage(Mathf.RoundToInt(damage));
        }

        public void TakeDamage(int damage)
        {
            _health.TakeDamage(damage, _playerTransform);

            if (_health.IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            enabled = false;
            StartCoroutine(DeathAnimationRoutine());
        }

        private IEnumerator DeathAnimationRoutine()
        {
            yield return _deathAnimation.PlayDeathAnimation();

            GameManager.Instance?.NotifyEnemyDefeated();
            Destroy(gameObject);
        }

        private void OnEnable()
        {
            _health.Initialize();
        }
    }
}
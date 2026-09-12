using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class EnemyHealth
    {
        private readonly EnemyHealthConfig config;

        private int _currentHealth;
        private bool _isDead;
        private float _targetFillAmount;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        public int CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        public EnemyHealth(EnemyHealthConfig config)
        {
            this.config = config;
        }

        public void Initialize()
        {
            _currentHealth = config.MaxHealth;
            _targetFillAmount = 1f;
            _isDead = false;

            if (config.SpriteRenderer != null)
                _originalColor = config.SpriteRenderer.color;

            if (config.HealthCanvasObject != null)
                config.HealthCanvasObject.SetActive(false);

            if (config.HealthFillImage != null)
                config.HealthFillImage.fillAmount = 1f;
        }

        public void Update()
        {
            if (config.HealthFillImage != null && config.HealthFillImage.canvas != null)
            {
                config.HealthFillImage.fillAmount = Mathf.Lerp(config.HealthFillImage.fillAmount, _targetFillAmount, Time.deltaTime * config.HealthLerpSpeed);

                Transform canvasTransform = config.HealthFillImage.canvas.transform;
                Vector3 canvasScale = canvasTransform.localScale;
                canvasScale.x = Mathf.Abs(canvasScale.x) * Mathf.Sign(config.Transform.localScale.x);
                canvasTransform.localScale = canvasScale;
            }
        }

        public void TakeDamage(int damage, Transform playerTransform)
        {
            if (_isDead || damage <= 0)
                return;

            _currentHealth -= damage;
            
            if (config.HealthCanvasObject != null && !config.HealthCanvasObject.activeSelf)
            {
                config.HealthCanvasObject.SetActive(true);
            }

            _targetFillAmount = Mathf.Clamp01((float)_currentHealth / config.MaxHealth);

            // Контролируемый отскок
            if (config.Rb != null && playerTransform != null)
            {
                float hitDirectionX = Mathf.Sign(config.Transform.position.x - playerTransform.position.x);
                if (hitDirectionX == 0) hitDirectionX = 1f;

                float controlledY = Mathf.Min(config.Rb.linearVelocity.y, 0.5f);
                config.Rb.linearVelocity = new Vector2(hitDirectionX * 2.5f, controlledY);
            }

            if (config.SpriteRenderer != null)
            {
                if (_flashCoroutine != null) 
                    CoroutineRunner.StopCoroutine(_flashCoroutine);
                _flashCoroutine = CoroutineRunner.StartCoroutine(FlashWhiteRoutine());
            }

            if (_currentHealth <= 0)
            {
                _isDead = true;
            }
        }

        private IEnumerator FlashWhiteRoutine()
        {
            config.SpriteRenderer.color = Color.white;
            yield return new WaitForSeconds(config.FlashDuration);
            config.SpriteRenderer.color = _originalColor;
        }
    }
}

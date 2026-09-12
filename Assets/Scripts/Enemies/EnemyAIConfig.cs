using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class EnemyMovementConfig
    {
        public float Speed { get; }
        public float JumpForce { get; }
        public float ExtraStepAfterEdge { get; }

        public EnemyMovementConfig(float speed, float jumpForce, float extraStepAfterEdge)
        {
            Speed = speed;
            JumpForce = jumpForce;
            ExtraStepAfterEdge = extraStepAfterEdge;
        }
    }

    public class EnemyJumpConfig
    {
        public float MaxJumpDistanceX { get; }
        public float MinJumpHeight { get; }
        public float JumpCooldown { get; }

        public EnemyJumpConfig(float maxJumpDistanceX, float minJumpHeight, float jumpCooldown)
        {
            MaxJumpDistanceX = maxJumpDistanceX;
            MinJumpHeight = minJumpHeight;
            JumpCooldown = jumpCooldown;
        }
    }

    public class EnemyRaycastConfig
    {
        public Transform GroundCheck { get; }
        public float GroundCheckRadius { get; }
        public LayerMask GroundLayer { get; }
        public Transform PlatformCheck { get; }
        public float PlatformCheckDistance { get; }
        public Transform DiagCheckPoint { get; }
        public float DiagCheckDistance { get; }

        public EnemyRaycastConfig(
            Transform groundCheck,
            float groundCheckRadius,
            LayerMask groundLayer,
            Transform platformCheck,
            float platformCheckDistance,
            Transform diagCheckPoint,
            float diagCheckDistance)
        {
            GroundCheck = groundCheck;
            GroundCheckRadius = groundCheckRadius;
            GroundLayer = groundLayer;
            PlatformCheck = platformCheck;
            PlatformCheckDistance = platformCheckDistance;
            DiagCheckPoint = diagCheckPoint;
            DiagCheckDistance = diagCheckDistance;
        }
    }

    public class EnemyHealthConfig
    {
        public int MaxHealth { get; }
        public Image HealthFillImage { get; }
        public GameObject HealthCanvasObject { get; }
        public float HealthLerpSpeed { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public float FlashDuration { get; }
        public Rigidbody2D Rb { get; }
        public Transform Transform { get; }

        public EnemyHealthConfig(
            int maxHealth,
            Image healthFillImage,
            GameObject healthCanvasObject,
            float healthLerpSpeed,
            SpriteRenderer spriteRenderer,
            float flashDuration,
            Rigidbody2D rb,
            Transform transform)
        {
            MaxHealth = maxHealth;
            HealthFillImage = healthFillImage;
            HealthCanvasObject = healthCanvasObject;
            HealthLerpSpeed = healthLerpSpeed;
            SpriteRenderer = spriteRenderer;
            FlashDuration = flashDuration;
            Rb = rb;
            Transform = transform;
        }
    }

    public class EnemyDeathConfig
    {
        public Transform Transform { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public GameObject HealthCanvasObject { get; }
        public Rigidbody2D Rb { get; }
        public Collider2D Collider { get; }

        public EnemyDeathConfig(
            Transform transform,
            SpriteRenderer spriteRenderer,
            GameObject healthCanvasObject,
            Rigidbody2D rb,
            Collider2D collider)
        {
            Transform = transform;
            SpriteRenderer = spriteRenderer;
            HealthCanvasObject = healthCanvasObject;
            Rb = rb;
            Collider = collider;
        }
    }
}

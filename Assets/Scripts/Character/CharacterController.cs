using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterStats))]
public class CharacterController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private Weapon currentWeapon; // Ссылка на текущее оружие

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStats stats;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private bool isGrounded;

    // Кэш хэшей параметров Animator
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Если оружие не привязано вручную в Inspector, ищем его на персонаже или его дочерних объектах
        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<Weapon>();
            Debug.Log($"CharacterController Awake: currentWeapon найден через GetComponentInChildren: {currentWeapon != null}");
        }
        else
        {
            Debug.Log($"CharacterController Awake: currentWeapon уже назначен в Inspector: {currentWeapon.name}");
        }
    }

    private void Update()
    {
        CheckGround();
        UpdateAnimator();
        UpdateSpriteFlip();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float speed = stats != null ? stats.MoveSpeed : 7f;

        // Мгновенная остановка по оси X при отсутствии ввода
        if (Mathf.Abs(moveInput.x) < 0.01f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        }

    }

    private void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float moveSpeedForAnimator = Mathf.Abs(moveInput.x) > 0.01f ? Mathf.Abs(rb.linearVelocity.x) : 0f;

        animator.SetFloat(SpeedHash, moveSpeedForAnimator);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
    }

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null) return;

        // Получаем позицию мыши
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Camera mainCamera = Camera.main ?? FindAnyObjectByType<Camera>();
        
        if (mainCamera == null) return;

        // Переводим в мировые координаты
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        
        // Переворачиваем спрайт если мышь слева от персонажа
        bool isMouseOnLeft = mouseWorldPosition.x < transform.position.x;
        spriteRenderer.flipX = isMouseOnLeft;
    }

    // ================= INPUT SYSTEM EVENTS =================

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        float force = stats != null ? stats.JumpForce : 12f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    // Вызывается автоматически при клике ЛКМ (экшен Attack в Player Input)
    public void OnAttack(InputValue value)
    {
        Debug.Log($"OnAttack вызван! isPressed: {value.isPressed}");
        
        if (!value.isPressed) return;

        Debug.Log($"OnAttack: currentWeapon != null: {currentWeapon != null}");
        
        if (currentWeapon != null)
        {
            Debug.Log($"OnAttack: вызываем Attack() на оружии: {currentWeapon.name}");
            currentWeapon.Attack();
        }
        else
        {
            Debug.LogWarning("CurrentWeapon не назначено в CharacterController!");
        }
    }

    // Вызывается при нажатии клавиши перезарядки (экшен Reload в Player Input)
    public void OnReload(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentWeapon != null)
        {
            currentWeapon.Reload();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
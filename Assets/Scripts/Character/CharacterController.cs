using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterStats))]
public class CharacterController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference attackAction;

    [Header("Combat & Weapons")]
    [SerializeField] private GameObject weaponPrefab;   // Префаб оружия из папки Project
    [SerializeField] private Transform weaponHoldPoint; // Точка (рука) на персонаже, куда спавнить оружие
    
    private Weapon currentWeapon; // Ссылка на уже созданный экземпляр оружия

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStats stats;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

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
        mainCamera = Camera.main ?? FindAnyObjectByType<Camera>();

        // Если задан префаб и точка крепления — спавним оружие динамически!
        if (weaponPrefab != null && weaponHoldPoint != null)
        {
            GameObject weaponObj = Instantiate(weaponPrefab, weaponHoldPoint);

            // Привязываем к руке и сбрасываем локальные координаты, чтобы не улетело в космос
            weaponObj.transform.localPosition = Vector3.zero;
            weaponObj.transform.localRotation = Quaternion.identity;

            currentWeapon = weaponObj.GetComponent<Weapon>();

            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        CheckGround();
        UpdateAnimator();
        UpdateSpriteFlip();
        UpdateVariableJump();
        UpdateAttack();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float speed = stats != null ? stats.MoveSpeed : 7f;

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
        if (groundCheck == null)
        {
            return;
        }
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

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Camera mainCamera = Camera.main ?? FindAnyObjectByType<Camera>();
        
        if (mainCamera == null) return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
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
        // Нажатие - прыгаем только если на земле
        if (value.isPressed && isGrounded)
        {
            float force = stats != null ? stats.JumpForce : 12f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        }
    }

    private void UpdateVariableJump()
    {
        // Читаем состояние кнопки напрямую через InputAction
        bool isJumpPressed = jumpAction != null && jumpAction.action.IsPressed();

        // Если кнопка отпущена и мы летим вверх - обнуляем скорость (перестаем подниматься)
        // Но не ускоряем падение - гравитация работает как обычно
        if (!isJumpPressed && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    public void OnAttack(InputValue value)
    {
        // Не используем этот метод, читаем состояние напрямую через InputActionReference
    }

    private void UpdateAttack()
    {
        // Читаем состояние кнопки напрямую через InputAction
        bool isAttackButtonPressed = attackAction != null && attackAction.action.IsPressed();

        if (isAttackButtonPressed && currentWeapon != null)
        {
            currentWeapon.Attack();
        }
    }

    public void OnReload(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentWeapon != null)
        {
            currentWeapon.Reload();
        }
    }

    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null || weaponHoldPoint == null)
            return;

        // Удаляем текущее оружие
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        // Спавним новое оружие
        GameObject weaponObj = Instantiate(newWeaponPrefab, weaponHoldPoint);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        currentWeapon = weaponObj.GetComponent<Weapon>();
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
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
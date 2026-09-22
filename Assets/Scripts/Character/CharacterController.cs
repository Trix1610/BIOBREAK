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

    [Header("Physics")]
    [SerializeField] private float fallGravityMultiplier = 2f;

    [Header("Jump")]
    [SerializeField, Range(0.1f, 1f)] private float jumpCutVelocityMultiplier = 0.5f;

    [Header("Combat & Weapons")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Transform weaponHoldPoint;

    private Weapon currentWeapon;

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStats stats;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction lookAction;

    // =========================================================
    // INPUT
    // =========================================================

    // Movement приходит из PlayerInput -> OnMove()
    private Vector2 moveInput;

    // =========================================================
    // MOVEMENT
    // =========================================================

    private bool isGrounded;
    private int jumpsRemaining;
    private bool suppressJumpUntilReleased;

    // =========================================================
    // ANIMATOR HASHES
    // =========================================================

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int VerticalVelocityHash =
        Animator.StringToHash("VerticalVelocity");

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            jumpAction = playerInput.actions.FindAction("Jump", throwIfNotFound: false);
            attackAction = playerInput.actions.FindAction("Attack", throwIfNotFound: false);
            lookAction = playerInput.actions.FindAction("Look", throwIfNotFound: false);
        }

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        // Не обращаемся здесь к stats.MaxJumps.
        // CharacterStats может ещё не выполнить Awake().

        SpawnStartingWeapon();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        int maxJumps = stats != null
            ? stats.MaxJumps
            : 1;

        jumpsRemaining = maxJumps;

        CheckGround();

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            suppressJumpUntilReleased = true;
            return;
        }

        CheckGround();

        UpdateJumpInput();

        UpdateAnimator();

        UpdateSpriteFlip();

        UpdateAttack();
    }

    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        Move();

        ApplyGravity();
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void Move()
    {
        float speed = stats != null
            ? stats.MoveSpeed
            : 7f;

        float horizontalVelocity =
            moveInput.x * speed;

        // Меняем только X.
        // Вертикальная скорость остаётся нетронутой.

        rb.linearVelocity = new Vector2(
            horizontalVelocity,
            rb.linearVelocity.y
        );
    }

    // =========================================================
    // GRAVITY
    // =========================================================

    private void ApplyGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity +=
                Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    // =========================================================
    // GROUND CHECK
    // =========================================================

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Только что приземлились.
        if (!wasGrounded && isGrounded)
        {
            ResetJumps();
        }
    }

    private void ResetJumps()
    {
        int maxJumps = stats != null
            ? stats.MaxJumps
            : 1;

        jumpsRemaining = maxJumps;
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void Jump()
    {
        int maxJumps = stats != null
            ? stats.MaxJumps
            : 1;

        // Если стоим на земле, но счётчик почему-то 0,
        // восстанавливаем его.

        if (isGrounded && jumpsRemaining <= 0)
        {
            jumpsRemaining = maxJumps;
        }

        if (jumpsRemaining <= 0)
            return;

        float jumpForce = stats != null
            ? stats.JumpForce
            : 12f;

        // Сохраняем горизонтальное движение.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpsRemaining--;
    }

    private void UpdateJumpInput()
    {
        if (jumpAction == null)
            return;

        // Submit в меню назначен на те же кнопки, что и Jump. После снятия
        // паузы ждём отпускания, чтобы это нажатие не превратилось в прыжок.
        if (suppressJumpUntilReleased)
        {
            if (jumpAction.IsPressed())
                return;

            suppressJumpUntilReleased = false;
        }

        if (jumpAction.WasPressedThisFrame())
        {
            Jump();
        }

        // WasReleasedThisFrame читается у самого действия Input System,
        // поэтому одинаково работает для Space, геймпада и rebinding.
        if (jumpAction.WasReleasedThisFrame() && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutVelocityMultiplier);
        }
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        float currentHorizontalSpeed =
            Mathf.Abs(rb.linearVelocity.x);

        animator.SetFloat(
            SpeedHash,
            currentHorizontalSpeed
        );

        animator.SetBool(
            IsGroundedHash,
            isGrounded
        );

        animator.SetFloat(
            VerticalVelocityHash,
            rb.linearVelocity.y
        );
    }

    // =========================================================
    // SPRITE FLIP
    // =========================================================

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null)
            return;

        if (lookAction == null)
            return;

        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        if (lookAction.activeControl?.device is Gamepad ||
            lookAction.activeControl?.device is Joystick)
        {
            if (Mathf.Abs(lookValue.x) < 0.01f)
                return;

            spriteRenderer.flipX = lookValue.x < 0f;
            return;
        }

        if (mainCamera == null)
            return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    lookValue.x,
                    lookValue.y,
                    0f
                )
            );

        bool isMouseOnLeft =
            mouseWorldPosition.x < transform.position.x;

        spriteRenderer.flipX = isMouseOnLeft;
    }

    // =========================================================
    // INPUT SYSTEM - MOVEMENT
    // =========================================================

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // =========================================================
    // INPUT SYSTEM - ATTACK
    // =========================================================


    private void UpdateAttack()
    {
        bool isAttackHeld = attackAction != null && attackAction.IsPressed();
        bool isAimingWithStick = lookAction != null &&
            (lookAction.activeControl?.device is Gamepad ||
             lookAction.activeControl?.device is Joystick) &&
            lookAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;

        if ((!isAttackHeld && !isAimingWithStick) || currentWeapon == null)
            return;

        currentWeapon.Attack();
    }

    // =========================================================
    // INPUT SYSTEM - RELOAD
    // =========================================================

    public void OnReload(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (currentWeapon == null)
            return;

        currentWeapon.Reload();
    }

    // =========================================================
    // WEAPON
    // =========================================================

    private void SpawnStartingWeapon()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning(
                "CharacterController: weaponPrefab is not assigned.",
                this
            );

            return;
        }

        if (weaponHoldPoint == null)
        {
            Debug.LogWarning(
                "CharacterController: weaponHoldPoint is not assigned.",
                this
            );

            return;
        }

        GameObject weaponObject =
            Instantiate(
                weaponPrefab,
                weaponHoldPoint
            );

        weaponObject.transform.localPosition =
            Vector3.zero;

        weaponObject.transform.localRotation =
            Quaternion.identity;

        currentWeapon =
            weaponObject.GetComponent<Weapon>();

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "CharacterController: weaponPrefab does not contain a Weapon component!",
                weaponObject
            );
        }
    }

    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null)
        {
            Debug.LogWarning(
                "CharacterController: newWeaponPrefab is null.",
                this
            );

            return;
        }

        if (weaponHoldPoint == null)
        {
            Debug.LogWarning(
                "CharacterController: weaponHoldPoint is not assigned.",
                this
            );

            return;
        }

        // Удаляем старое оружие.

        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
        }

        // Создаём новое.

        GameObject weaponObject =
            Instantiate(
                newWeaponPrefab,
                weaponHoldPoint
            );

        weaponObject.transform.localPosition =
            Vector3.zero;

        weaponObject.transform.localRotation =
            Quaternion.identity;

        currentWeapon =
            weaponObject.GetComponent<Weapon>();

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "CharacterController: new weapon prefab does not contain a Weapon component!",
                weaponObject
            );
        }
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}

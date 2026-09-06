using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private Weapon currentWeapon; // Ссылка на оружие (пистолет)

    public StateMachine StateMachine { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public CharacterStats Stats { get; private set; }
    public Weapon CurrentWeapon => currentWeapon; 
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public int CurrentJumps { get; set; }

    private bool wasGrounded;
    private bool _isInputLocked = false;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Stats = GetComponent<CharacterStats>();
        StateMachine = new StateMachine();

        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<Weapon>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ROOM_00" || scene.name == "GAME")
            return;

        StartCoroutine(LockInputRoutine(0.20f));
    }

    private System.Collections.IEnumerator LockInputRoutine(float duration)
    {
        _isInputLocked = true;
        // Ввод НЕ сбрасываем, чтобы состояние нажатой клавиши сохранялось в памяти!
        
        yield return new WaitForSeconds(duration);

        _isInputLocked = false;
    }

    private void Start()
    {
        StateMachine.ChangeState(new IdleState(this, Stats));
    }

    private void Update()
    {
        CheckGrounded();
        StateMachine.Update();

        // Если игрок отпустил пробел во время полета вверх
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            if (Rigidbody.linearVelocity.y > 0)
            {
                Rigidbody.linearVelocity = new Vector2(
                    Rigidbody.linearVelocity.x, 
                    Rigidbody.linearVelocity.y * 0.5f
                );
            }
        }
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }

    private void CheckGrounded()
    {
        IsGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(
                         groundCheck.position,
                         groundCheckRadius,
                         groundLayer
                     );

        if (IsGrounded && !wasGrounded)
        {
            CurrentJumps = Stats.MaxJumps;
        }

        wasGrounded = IsGrounded;
    }

    public void OnMove(InputValue value)
    {
        // Не блокируем считывание ввода, даже если идет микро-пауза при входе в комнату
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (_isInputLocked) return;

        if (!value.isPressed)
            return;

        bool isGrounded = groundCheck != null &&
                          Physics2D.OverlapCircle(
                              groundCheck.position,
                              groundCheckRadius,
                              groundLayer
                          );

        bool canJump = (isGrounded && Stats.MaxJumps > 0) || CurrentJumps > 0;

        if (canJump)
        {
            CurrentJumps--;
            StateMachine.ChangeState(new JumpState(this, Stats));
        }
    }

    public void OnAttack(InputValue value)
    {
        if (_isInputLocked) return;

        if (value.isPressed && currentWeapon != null)
        {
            currentWeapon.Attack();
        }
    }

    public void HandleMovement()
    {
        // Пока идет блокировка (0.25с), глушим скорость движения в ноль, 
        // но игрок может продолжать удерживать клавишу «Вправо»
        float targetX = _isInputLocked ? 0f : MoveInput.x * Stats.MoveSpeed;

        Rigidbody.linearVelocity = new Vector2(
            targetX,
            Rigidbody.linearVelocity.y
        );
    }

    public void StopHorizontalMovement()
    {
        Rigidbody.linearVelocity = new Vector2(
            0,
            Rigidbody.linearVelocity.y
        );
    }

    public void HandleJump()
    {
        Rigidbody.linearVelocity = new Vector2(
            Rigidbody.linearVelocity.x,
            Stats.JumpForce
        );
    }

    public void TakeDamage(int damage)
    {
        if (Stats != null)
        {
            Stats.TakeDamage(damage);
        }
    }
}
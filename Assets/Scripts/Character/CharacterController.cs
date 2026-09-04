using UnityEngine;
using UnityEngine.InputSystem;

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
    public Weapon CurrentWeapon => currentWeapon; // Публичное свойство для доступа из состояний/других скриптов
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public int CurrentJumps { get; set; }

    private bool wasGrounded;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Stats = GetComponent<CharacterStats>();
        StateMachine = new StateMachine();

        // Если оружие не перетащили в инспектор вручную, пытаемся найти его автоматически на дочерних объектах
        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<Weapon>();
        }
    }

    private void Start()
    {
        StateMachine.ChangeState(new IdleState(this, Stats));
    }

    private void Update()
    {
        CheckGrounded();
        StateMachine.Update();
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

        // Сбрасываем прыжки только при фактическом приземлении (переход из воздуха на землю)
        if (IsGrounded && !wasGrounded)
        {
            CurrentJumps = Stats.MaxJumps;
        }

        wasGrounded = IsGrounded;
    }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
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

    // Метод для обработки стрельбы через Player Input (срабатывает на ЛКМ, если настроено действие "Attack")
    public void OnAttack(InputValue value)
    {
        Debug.Log($"OnAttack вызван! IsPressed: {value.isPressed}, Weapon: {currentWeapon}");

        if (value.isPressed && currentWeapon != null)
        {
            currentWeapon.Attack();
        }
    }

    public void HandleMovement()
    {
        Rigidbody.linearVelocity = new Vector2(
            MoveInput.x * Stats.MoveSpeed,
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

    public void TakeDamage(float damage)
    {
        Stats.TakeDamage(damage);
        StateMachine.ChangeState(new HurtState(this, Stats));

        if (Stats.CurrentHealth <= 0)
        {
            StateMachine.ChangeState(new DeathState(this, Stats));
        }
    }
}
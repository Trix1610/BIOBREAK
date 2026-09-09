using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Core;

public class CharacterController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private Weapon currentWeapon;

    public StateMachine StateMachine { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public CharacterStats Stats { get; private set; }
    public Weapon CurrentWeapon => currentWeapon;
    public Vector2 MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public int CurrentJumps { get; set; }

    private bool wasGrounded;
    private bool _isInputLocked = false;
    private WeaponController weaponController;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Stats = GetComponent<CharacterStats>();
        StateMachine = new StateMachine();

        if (currentWeapon == null)
        {
            currentWeapon = GetComponentInChildren<Weapon>();
        }

        weaponController = new WeaponController(currentWeapon);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (Stats != null)
            Stats.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Stats != null)
            Stats.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        StateMachine.ChangeState(new DeathState(this, Stats));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneNames.StartRoom || scene.name == SceneNames.Game)
            return;

        StartCoroutine(LockInputRoutine(0.20f));
    }

    private System.Collections.IEnumerator LockInputRoutine(float duration)
    {
        _isInputLocked = true;
        yield return new WaitForSeconds(duration);
        _isInputLocked = false;
    }

    private void Start()
    {
        CurrentJumps = Stats.MaxJumps;
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

        if (value.isPressed)
        {
            weaponController.Attack();
        }
    }

    public void HandleMovement()
    {
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
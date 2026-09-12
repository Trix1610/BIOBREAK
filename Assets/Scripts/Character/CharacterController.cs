using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Core;
using Character;

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
    public bool IsGrounded => _groundCheck.IsGrounded;
    public int CurrentJumps { get; set; }

    private bool _isInputLocked = false;
    private WeaponController weaponController;
    private CharacterGroundCheck _groundCheck;
    private CharacterMovement _movement;

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

        // Регистрируем игрока в PlayerReference
        if (PlayerReference.Instance != null)
        {
            PlayerReference.Instance.SetPlayer(gameObject);
        }

        // Создаем компоненты
        var groundCheckConfig = new CharacterGroundCheckConfig(groundCheck, groundCheckRadius, groundLayer);
        var movementConfig = new CharacterMovementConfig(Rigidbody, Stats);

        _groundCheck = new CharacterGroundCheck(groundCheckConfig);
        _movement = new CharacterMovement(movementConfig, _groundCheck);
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
        _groundCheck.Update();
        CurrentJumps = _groundCheck.CheckLanding(CurrentJumps, Stats.MaxJumps);
        _movement.HandleJumpRelease();
        StateMachine.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
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

        bool canJump = _movement.CanJump(CurrentJumps, Stats.MaxJumps);

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
        _movement.HandleMovement(MoveInput, _isInputLocked);
    }

    public void StopHorizontalMovement()
    {
        _movement.StopHorizontalMovement();
    }

    public void HandleJump()
    {
        _movement.HandleJump();
    }

    public void TakeDamage(int damage)
    {
        if (Stats != null)
        {
            Stats.TakeDamage(damage);
        }
    }
}
using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float BaseSpeed { get; set; } = 200.0f;
	[Export] public float JumpVelocity { get; set; } = -600.0f;
	
	[Export] public int MaxHealth { get; set; } = 8;
	[Export] public int CurrentHealth { get; set; } = 6;
	[Export] private NodePath healthUIPath;
	private HealthUI _healthUI;

	private bool _isKnockedBack = false;
	private float _knockbackTimer = 0.0f;
	private bool _isInvulnerable = false;
	
	[Export] public float KnockbackForceX { get; set; } = 300.0f;
	[Export] public float KnockbackForceY { get; set; } = -200.0f;

	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _jumpsLeft = 0;
	public OrganManager OrganManager { get; private set; }

	private Weapon _currentWeapon;
	[Export] public PackedScene WeaponSceneTemplate { get; set; }
	[Export] public WeaponData StartingWeaponData { get; set; }

	public override void _Ready()
	{
		AddToGroup("Player");

		OrganManager = GetNodeOrNull<OrganManager>("OrganManager");
		if (OrganManager == null)
		{
			OrganManager = new OrganManager { Name = "OrganManager" };
			AddChild(OrganManager);
		}

		// Прямое обращение к глобальному HealthUI при спавне!
		if (HealthUI.Instance != null) // Если используешь синглтон, либо просто обращайся к автозагрузке
		{
			// Если в Autoload имя узла HealthUI, то можно использовать его напрямую:
			// HealthUI.Initialize(this); (но проще через синглтон ниже)
		}

		EquipStartingWeapon();
		CallDeferred(nameof(PositionAtSpawnPoint));
	}

	private void EquipStartingWeapon()
	{
		if (WeaponSceneTemplate == null)
		{
			WeaponSceneTemplate = GD.Load<PackedScene>("res://scenes/player/Weapon.tscn");
		}
		if (StartingWeaponData == null)
		{
			StartingWeaponData = GD.Load<WeaponData>("res://resources/weapons/pistol.tres");
		}

		if (WeaponSceneTemplate != null && StartingWeaponData != null)
		{
			Node weaponNode = WeaponSceneTemplate.Instantiate();
			if (weaponNode is Weapon weapon)
			{
				_currentWeapon = weapon;
				_currentWeapon.Data = StartingWeaponData;
				AddChild(_currentWeapon);
				_currentWeapon.Position = new Vector2(15, 0); 
			}
		}
	}

	private void PositionAtSpawnPoint()
	{
		if (GameManager.Instance == null) return;
		string targetPointName = GameManager.Instance.TargetSpawnPoint;
		Marker2D spawnPoint = GetParent().FindChild(targetPointName, recursive: true, owned: false) as Marker2D;
		if (spawnPoint != null)
		{
			GlobalPosition = spawnPoint.GlobalPosition;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isKnockedBack)
		{
			_knockbackTimer -= (float)delta;
			Vector2 knockbackVel = Velocity;
			knockbackVel.Y += gravity * (float)delta;
			knockbackVel.X = Mathf.MoveToward(Velocity.X, 0, BaseSpeed * 0.5f * (float)delta);
			Velocity = knockbackVel;
			MoveAndSlide();

			if (_knockbackTimer <= 0)
			{
				_isKnockedBack = false;
			}

			CheckEnemyCollisionForKnockback();
			return; 
		}

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
		}
		else
		{
			_jumpsLeft = 1 + OrganManager.GetTotalExtraJumps();
		}

		if (Input.IsActionJustPressed("ui_accept") && _jumpsLeft > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpsLeft--;
		}

		if (Input.IsActionJustReleased("ui_accept") && velocity.Y < 0)
		{
			velocity.Y *= 0.5f;
		}

		float direction = Input.GetAxis("ui_left", "ui_right");
		float currentSpeed = BaseSpeed + OrganManager.GetTotalBonusSpeed();

		if (direction != 0)
		{
			velocity.X = direction * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();

		CheckEnemyCollisionForKnockback();
	}

	private void CheckEnemyCollisionForKnockback()
	{
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is Enemy enemy)
			{
				ApplyKnockback(enemy.GlobalPosition);
				break;
			}
		}
	}

	private void ApplyKnockback(Vector2 enemyGlobalPosition)
	{
		Velocity = new Vector2(0, Velocity.Y);
		_isKnockedBack = true;
		_knockbackTimer = 0.35f;

		float knockbackDir = GlobalPosition.X > enemyGlobalPosition.X ? 1.0f : -1.0f;
		Velocity = new Vector2(KnockbackForceX * knockbackDir, KnockbackForceY);
	}

	public override void _Input(InputEvent @event)
	{
		bool isMouseClick = @event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left;
		bool isFKey = @event is InputEventKey k && k.Pressed && !k.Echo && k.Keycode == Key.F;

		if ((isMouseClick || isFKey) && _currentWeapon != null)
		{
			_currentWeapon.Shoot(GlobalPosition, GetGlobalMousePosition());
		}
	}

	public void TakeDamage(int damage, Vector2 enemyGlobalPosition)
	{
		if (_isInvulnerable) return;

		CurrentHealth -= damage;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		// Прямой вызов глобального UI!
		if (HealthUI.Instance != null)
		{
			HealthUI.Instance.UpdateHealth(CurrentHealth);
		}

		GD.Print($"Игрок получил урон! Здоровье: {CurrentHealth}");
		StartInvulnerability(0.5f);

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private async void StartInvulnerability(float duration)
	{
		_isInvulnerable = true;
		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		
		float elapsed = 0f;
		while (elapsed < duration)
		{
			if (sprite != null) sprite.Visible = !sprite.Visible;
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			elapsed += 0.1f;
		}

		if (sprite != null) sprite.Visible = true;
		_isInvulnerable = false;
	}

	private void Die()
	{
		GD.Print("Игрок погиб!");
	}
}

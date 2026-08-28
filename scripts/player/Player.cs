using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float BaseSpeed { get; set; } = 200.0f;
	[Export] public float JumpVelocity { get; set; } = -600.0f; // Сила прыжка (в Godot вверх — это минус)
	
	// Здоровье игрока
	[Export] public int MaxHealth { get; set; } = 6;
	[Export] public int CurrentHealth { get; set; } = 6;
	[Export] private NodePath healthUIPath;
	private HealthUI _healthUI;

	// Переменные для отскока при уроне
	private bool _isKnockedBack = false;
	private float _knockbackTimer = 0.0f;
	[Export] public float KnockbackForceX { get; set; } = 300.0f; // Сила отскока вбок
	[Export] public float KnockbackForceY { get; set; } = -200.0f; // Сила отскока вверх

	// Получаем значение гравитации из настроек проекта Godot
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _jumpsLeft = 0;
	public OrganManager OrganManager { get; private set; }

	// --- СИСТЕМА ОРУЖИЯ ---
	private Weapon _currentWeapon;
	[Export] public PackedScene WeaponSceneTemplate { get; set; }   // Ссылка на Weapon.tscn
	[Export] public WeaponData StartingWeaponData { get; set; }     // Ссылка на pistol.tres

	public override void _Ready()
	{
		OrganManager = GetNodeOrNull<OrganManager>("OrganManager");
		if (OrganManager == null)
		{
			OrganManager = new OrganManager { Name = "OrganManager" };
			AddChild(OrganManager);
		}

		if (!healthUIPath.IsEmpty)
		{
			_healthUI = GetNodeOrNull<HealthUI>(healthUIPath);
		}
		
		CurrentHealth = MaxHealth;
		if (_healthUI != null)
		{
			_healthUI.UpdateHealth(CurrentHealth);
		}

		// Выдаем стартовое оружие при запуске
		EquipStartingWeapon();

		CallDeferred(nameof(PositionAtSpawnPoint));
	}

	private void EquipStartingWeapon()
	{
		// Если в инспекторе не перетащили сцену оружия или данные, загружаем их автоматически
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
				// Смещаем оружие немного вправо/вперед от центра игрока
				_currentWeapon.Position = new Vector2(15, 0); 
				
				GD.Print("Стартовое оружие экипировано: " + StartingWeaponData.WeaponName);
			}
		}
		else
		{
			GD.PrintErr("ОШИБКА: Не удалось выдать стартовое оружие! Проверь пути к Weapon.tscn и pistol.tres.");
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
		// Обработка таймера отскока
		if (_isKnockedBack)
		{
			_knockbackTimer -= (float)delta;
			
			Vector2 knockbackVel = Velocity;
			knockbackVel.Y += gravity * (float)delta;
			Velocity = knockbackVel;
			
			MoveAndSlide();

			if (_knockbackTimer <= 0)
			{
				_isKnockedBack = false;
			}
			return; 
		}

		Vector2 velocity = Velocity;

		// 1. Гравитация
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
		}
		else
		{
			_jumpsLeft = 1 + OrganManager.GetTotalExtraJumps();
		}

		// 2. Прыжок
		if (Input.IsActionJustPressed("ui_accept") && _jumpsLeft > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpsLeft--;
		}

		// Обрезка высоты прыжка при отпускании кнопки
		if (Input.IsActionJustReleased("ui_accept") && velocity.Y < 0)
		{
			velocity.Y *= 0.5f;
		}

		// 3. Движение
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
	}

	// Обработка одиночных нажатий для стрельбы (чтобы пули не летели каждый кадр потоком)
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
		CurrentHealth -= damage;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		if (_healthUI != null)
		{
			_healthUI.UpdateHealth(CurrentHealth);
		}

		// Сбрасываем X-скорость, чтобы бег навстречу врагу не гасил отскок
		Velocity = new Vector2(0, Velocity.Y);

		// Включаем отскок и блокировку управления на 0.25 секунды
		_isKnockedBack = true;
		_knockbackTimer = 0.25f;

		// Считаем направление отскока от врага
		float knockbackDir = GlobalPosition.X > enemyGlobalPosition.X ? 1.0f : -1.0f;
		Velocity = new Vector2(KnockbackForceX * knockbackDir, KnockbackForceY);

		GD.Print($"Игрок получил урон! Здоровье: {CurrentHealth}");

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Игрок погиб!");
	}
}

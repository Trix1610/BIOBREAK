using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float BaseSpeed { get; set; } = 200.0f;
	[Export] public float JumpVelocity { get; set; } = -600.0f;
	
	[Export] public int MaxHealth { get; set; } = 8;
	public int CurrentHealth { get; private set; } = 8;

	public void SetHealth(int health)
	{
		CurrentHealth = health;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		if (RunManager.Instance != null)
		{
			RunManager.Instance.CurrentPlayerHealth = CurrentHealth;
		}
	}
	
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

	private WeaponBase _currentWeapon;
	[Export] public PackedScene WeaponSceneTemplate { get; set; }
	[Export] public WeaponData StartingWeaponData { get; set; }

	private AnimatedSprite2D _animatedSprite;
	
	// ===== ДОБАВЛЕНО: запоминаем последнее направление =====
	private float _lastDirection = 1.0f; // 1 - вправо, -1 - влево

	public override void _Ready()
	{
		AddToGroup("Player");

		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		if (_animatedSprite == null)
		{
			GD.PrintErr("[Player] AnimatedSprite2D не найден!");
		}

		if (RunManager.Instance != null && RunManager.Instance.IsRunActive)
		{
			CurrentHealth = RunManager.Instance.CurrentPlayerHealth;
		}
		else
		{
			CurrentHealth = MaxHealth;
		}

		GD.Print($"[Player _Ready] Игрок создан. MaxHealth: {MaxHealth}, CurrentHealth: {CurrentHealth}");

		OrganManager = GetNodeOrNull<OrganManager>("OrganManager");
		if (OrganManager == null)
		{
			OrganManager = new OrganManager { Name = "OrganManager" };
			AddChild(OrganManager);
		}

		EquipStartingWeapon();
		CallDeferred(nameof(PositionAtSpawnPoint));
		CallDeferred(nameof(NotifyHealthUI));
	}

	private void NotifyHealthUI()
	{
		if (HealthUI.Instance != null)
		{
			GD.Print("[Player] Уведомляем HealthUI о своем появлении.");
			HealthUI.Instance.SetupForPlayer(this);
		}
	}

	private void EquipStartingWeapon()
	{
		if (RunManager.Instance != null && RunManager.Instance.CurrentWeaponData != null)
		{
			StartingWeaponData = RunManager.Instance.CurrentWeaponData;
		}
		else if (StartingWeaponData == null)
		{
			StartingWeaponData = GD.Load<WeaponData>("res://resources/weapons/projectile/pistol.tres");
		}

		if (StartingWeaponData != null)
		{
			WeaponBase weapon = CreateWeaponByType(StartingWeaponData.WeaponType);
			
			if (weapon != null)
			{
				weapon.Data = StartingWeaponData; // ← ЭТО ВАЖНО!
				_currentWeapon = weapon;
				AddChild(_currentWeapon);
				
				// Позиция оружия в руках игрока
				_currentWeapon.Position = new Vector2(25, -5); // ← подбери под свой спрайт
			}
		}
	}

	private WeaponBase CreateWeaponByType(WeaponType weaponType)
	{
		return weaponType switch
		{
			WeaponType.AutomaticWeapon => new AutomaticWeapon(),
			WeaponType.ShotgunWeapon => new ShotgunWeapon(),
			WeaponType.LaserWeapon => new LaserWeapon(),
			WeaponType.RailgunWeapon => new RailgunWeapon(),
			WeaponType.SonicWeapon => new SonicWeapon(),
			WeaponType.ExplosiveWeapon => new ExplosiveWeapon(),
			_ => new SingleShotWeapon()
		};
	}

	public void EquipWeapon(WeaponData newWeaponData)
	{
		if (_currentWeapon != null)
		{
			_currentWeapon.QueueFree();
		}

		WeaponBase weapon = CreateWeaponByType(newWeaponData.WeaponType);
		
		if (weapon != null)
		{
			weapon.Data = newWeaponData;
			_currentWeapon = weapon;
			AddChild(_currentWeapon);
			_currentWeapon.Position = new Vector2(15, 0);
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

		if (Input.IsActionJustPressed("jump") && _jumpsLeft > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpsLeft--;
		}

		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
		{
			velocity.Y *= 0.5f;
		}

		float direction = Input.GetAxis("ui_left", "ui_right");
		float currentSpeed = BaseSpeed + OrganManager.GetTotalBonusSpeed();

		if (direction != 0)
		{
			velocity.X = direction * currentSpeed;
			_lastDirection = direction;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateAnimation(direction);

		// ===== ПРОДВИНУТОЕ УПРАВЛЕНИЕ ОРУЖИЕМ =====
		UpdateWeapon();

		if (Input.IsMouseButtonPressed(MouseButton.Left) && _currentWeapon is AutomaticWeapon autoWeapon)
		{
			autoWeapon.ContinueFiring(GlobalPosition, GetGlobalMousePosition());
		}

		CheckEnemyCollisionForKnockback();
	}

	// ===== НОВЫЙ МЕТОД ДЛЯ УПРАВЛЕНИЯ ОРУЖИЕМ =====
	private void UpdateWeapon()
	{
		if (_currentWeapon == null) return;

		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 directionToMouse = (mousePos - GlobalPosition).Normalized();
		
		// Расстояние от игрока до оружия (можно будет настраивать для каждого оружия)
		float weaponDistance = 6f;
		
		// Позиция оружия (вращается вокруг игрока)
		_currentWeapon.Position = directionToMouse * weaponDistance;
		
		// Вращение оружия в сторону мыши
		_currentWeapon.Rotation = directionToMouse.Angle();
		
		// ===== УМНАЯ СИСТЕМА ЗЕРКАЛИРОВАНИЯ =====
		// Определяем, смотрит ли оружие влево
		float angle = directionToMouse.Angle();
		bool isFlipped = angle > Mathf.Pi / 2 || angle < -Mathf.Pi / 2;
		
		if (isFlipped)
		{
			// Оружие смотрит влево - зеркалим по Y
			_currentWeapon.Scale = new Vector2(1, -1);
			
			// Если оружие слева, корректируем позицию, чтобы оно не перекрывало персонажа
			// Можно добавить смещение в зависимости от оружия
			if (_currentWeapon is WeaponBase weapon && weapon.Data != null)
			{
				// Немного смещаем оружие вверх, чтобы оно не перекрывало руку
				_currentWeapon.Position += new Vector2(0, -2);
			}
		}
		else
		{
			// Оружие смотрит вправо - нормальный масштаб
			_currentWeapon.Scale = new Vector2(1, 1);
		}
		
		// ===== ОТЛАДКА (можно закомментировать) =====
		// GD.Print($"Оружие: позиция {_currentWeapon.Position}, угол {Mathf.RadToDeg(angle):F1}°");
	}

	// ===== ДОПОЛНИТЕЛЬНО: МЕТОД ДЛЯ СТРЕЛЬБЫ =====
	private void ShootWeapon()
	{
		if (_currentWeapon == null) return;
		
		Vector2 mousePos = GetGlobalMousePosition();
		_currentWeapon.Shoot(GlobalPosition, mousePos);
		
		// Если оружие не автоматическое, можно добавить эффект отдачи
		if (_currentWeapon is not AutomaticWeapon)
		{
			// Эффект отдачи (опционально)
			// Например, небольшая задержка перед следующим выстрелом
		}
	}

	private void UpdateAnimation(float direction)
	{
		if (_animatedSprite == null || _animatedSprite.SpriteFrames == null) return;

		bool isMoving = direction != 0;
		bool isShooting = Input.IsMouseButtonPressed(MouseButton.Left);
		
		// ===== ИЗМЕНЕНО: используем _lastDirection когда стоим =====
		float dirForAnimation = isMoving ? direction : _lastDirection;
		string dir = dirForAnimation < 0 ? "left" : "right";
		
		if (isShooting)
		{
			string animName = $"shoot_{dir}";
			if (_animatedSprite.SpriteFrames.HasAnimation(animName))
			{
				if (_animatedSprite.Animation != animName)
				{
					_animatedSprite.Play(animName);
				}
			}
			else if (_animatedSprite.SpriteFrames.HasAnimation("shoot"))
			{
				if (_animatedSprite.Animation != "shoot")
				{
					_animatedSprite.Play("shoot");
				}
				_animatedSprite.FlipH = dirForAnimation < 0;
			}
			return;
		}
		
		if (isMoving)
		{
			string animName = $"walk_{dir}";
			if (_animatedSprite.SpriteFrames.HasAnimation(animName))
			{
				if (_animatedSprite.Animation != animName)
				{
					_animatedSprite.Play(animName);
				}
			}
			else if (_animatedSprite.SpriteFrames.HasAnimation("walk"))
			{
				if (_animatedSprite.Animation != "walk")
				{
					_animatedSprite.Play("walk");
				}
				_animatedSprite.FlipH = dirForAnimation < 0;
			}
			return;
		}
		
		// Покой - используем последнее направление
		string idleAnim = $"idle_{dir}";
		if (_animatedSprite.SpriteFrames.HasAnimation(idleAnim))
		{
			if (_animatedSprite.Animation != idleAnim)
			{
				_animatedSprite.Play(idleAnim);
			}
		}
		else if (_animatedSprite.SpriteFrames.HasAnimation("idle"))
		{
			if (_animatedSprite.Animation != "idle")
			{
				_animatedSprite.Play("idle");
			}
			_animatedSprite.FlipH = dirForAnimation < 0;
		}
		else
		{
			_animatedSprite.Stop();
		}
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
			
			if (_currentWeapon is not AutomaticWeapon && _animatedSprite != null)
			{
				// ===== ИЗМЕНЕНО: используем _lastDirection =====
				string dir = _lastDirection < 0 ? "left" : "right";
				string animName = $"shoot_{dir}";
				
				if (_animatedSprite.SpriteFrames.HasAnimation(animName))
				{
					_animatedSprite.Play(animName);
				}
				else if (_animatedSprite.SpriteFrames.HasAnimation("shoot"))
				{
					_animatedSprite.Play("shoot");
					_animatedSprite.FlipH = _lastDirection < 0;
				}
			}
		}

		if (@event is InputEventMouseButton mbReleased && !mbReleased.Pressed && mbReleased.ButtonIndex == MouseButton.Left)
		{
			if (_currentWeapon is AutomaticWeapon autoWeapon)
			{
				autoWeapon.StopFiring();
			}
		}
	}

	public void TakeDamage(int damage, Vector2 enemyGlobalPosition)
	{
		if (_isInvulnerable) return;

		CurrentHealth -= damage;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		if (RunManager.Instance != null)
		{
			RunManager.Instance.CurrentPlayerHealth = CurrentHealth;
		}

		if (HealthUI.Instance != null)
		{
			HealthUI.Instance.UpdateHealth(CurrentHealth);
		}

		GD.Print($"Игрок получил урон! Здоровье: {CurrentHealth}");
		StartInvulnerability(0.5f);

		// ===== ИЗМЕНЕНО: используем _lastDirection =====
		if (_animatedSprite != null)
		{
			string dir = _lastDirection < 0 ? "left" : "right";
			string animName = $"hurt_{dir}";
			
			if (_animatedSprite.SpriteFrames.HasAnimation(animName))
			{
				_animatedSprite.Play(animName);
			}
			else if (_animatedSprite.SpriteFrames.HasAnimation("hurt"))
			{
				_animatedSprite.Play("hurt");
				_animatedSprite.FlipH = _lastDirection < 0;
			}
		}

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private async void StartInvulnerability(float duration)
	{
		_isInvulnerable = true;
		var sprite = _animatedSprite as CanvasItem;
		if (sprite == null)
		{
			sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		}
		
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

		// ===== ИЗМЕНЕНО: используем _lastDirection =====
		if (_animatedSprite != null)
		{
			string dir = _lastDirection < 0 ? "left" : "right";
			string animName = $"die_{dir}";
			
			if (_animatedSprite.SpriteFrames.HasAnimation(animName))
			{
				_animatedSprite.Play(animName);
			}
			else if (_animatedSprite.SpriteFrames.HasAnimation("die"))
			{
				_animatedSprite.Play("die");
				_animatedSprite.FlipH = _lastDirection < 0;
			}
		}

		if (IsInGroup("Player"))
		{
			RemoveFromGroup("Player");
		}

		if (RunManager.Instance != null)
		{
			RunManager.Instance.ShowGameOver();
		}

		SetPhysicsProcess(false);
		SetProcessInput(false);

		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (sprite != null) sprite.Visible = false;

		CollisionLayer = 0;
		CollisionMask = 0;

		CallDeferred(nameof(DestroyPlayer));
	}

	private void DestroyPlayer()
	{
		QueueFree();
	}
}

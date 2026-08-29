using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public int MaxHealth { get; set; } = 30;
	[Export] public float Speed { get; set; } = 80.0f;
	[Export] public float JumpForce { get; set; } = -300.0f; // Сила прыжка (отрицательная, так как верх в Godot — это минус)
	
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public int CurrentHealth { get; private set; }

	private Node2D _player;
	private ProgressBar _healthBar;
	
	private float _damageCooldown = 0.0f;
	private const float AttackCooldownTime = 0.4f;

	// Таймер для случайной паузы между прыжками
	private float _jumpTimer = 0.0f;
	private float _nextJumpInterval = 1.5f;

	[Export] 
	private Godot.Collections.Array<string> PossibleWeaponDrops { get; set; } = new()
	{
		"res://resources/weapons/projectile/machingun.tres",
		"res://resources/weapons/projectile/smg.tres"
	};

	[Export] 
	private PackedScene WeaponPickupScene { get; set; }

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		AddToGroup("Enemy");

		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		SetupHealthBar();
		
		ResetJumpTimer();
	}

	private void ResetJumpTimer()
	{
		// Случайный интервал между прыжками (от 1 до 2.5 секунд), чтобы они прыгали хаотично
		_nextJumpInterval = (float)GD.RandRange(1.0f, 2.5f);
		_jumpTimer = 0.0f;
	}

	private void SetupHealthBar()
	{
		_healthBar = new ProgressBar();
		_healthBar.MaxValue = MaxHealth;
		_healthBar.Value = CurrentHealth;
		_healthBar.ShowPercentage = false;
		
		_healthBar.CustomMinimumSize = new Vector2(50, 4);
		_healthBar.Position = new Vector2(-15, -24);
		_healthBar.Scale = new Vector2(0.6f, 0.6f);

		var styleFill = new StyleBoxFlat();
		styleFill.BgColor = Colors.Red;
		_healthBar.AddThemeStyleboxOverride("fill", styleFill);

		var styleBg = new StyleBoxFlat();
		styleBg.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
		_healthBar.AddThemeStyleboxOverride("background", styleBg);

		AddChild(_healthBar);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_damageCooldown > 0)
		{
			_damageCooldown -= (float)delta;
		}

		Vector2 velocity = Velocity;

		// Гравитация
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}
		else
		{
			// Когда на полу, считаем время до следующего прыжка
			_jumpTimer += (float)delta;
			if (_jumpTimer >= _nextJumpInterval)
			{
				velocity.Y = JumpForce; // Совершаем прыжок!
				ResetJumpTimer();       // Сбрасываем таймер для следующего прыжка
			}
		}

		// Движение за игроком по горизонтали
		if (_player != null && GodotObject.IsInstanceValid(_player))
		{
			float direction = Mathf.Sign(_player.GlobalPosition.X - GlobalPosition.X);
			velocity.X = direction * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * (float)delta);
			_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		}

		Velocity = velocity;
		MoveAndSlide();

		// Нанесение урона игроку при контакте по кулдауну
		if (_damageCooldown <= 0)
		{
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				var collision = GetSlideCollision(i);
				if (collision.GetCollider() is Player player)
				{
					player.TakeDamage(1, GlobalPosition);
					_damageCooldown = AttackCooldownTime;
					break; 
				}
			}
		}
	}

	public void TakeDamage(int damage)
	{
		CurrentHealth -= Mathf.Min(damage, CurrentHealth);
		GD.Print($"[Enemy] Получен урон: {damage}. Осталось HP: {CurrentHealth}");

		if (_healthBar != null)
		{
			_healthBar.Value = CurrentHealth;
		}

		SpawnDamagePopup(damage);

		Modulate = Colors.Red;
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => {
			if (GodotObject.IsInstanceValid(this)) Modulate = Colors.White;
		};

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	private void SpawnDamagePopup(int damage)
	{
		var popup = new Label();
		popup.Text = $"-{damage}";
		popup.AddThemeColorOverride("font_color", Colors.Yellow);
		popup.AddThemeFontSizeOverride("font_size", 12);

		GetTree().CurrentScene.AddChild(popup);
		popup.GlobalPosition = GlobalPosition + new Vector2(-10, -35);

		var tween = GetTree().CreateTween().SetParallel(true);

		tween.TweenProperty(popup, "global_position", popup.GlobalPosition + new Vector2(0, -15), 0.4f);
		tween.TweenProperty(popup, "modulate:a", 0.0f, 0.4f);

		tween.Chain().TweenCallback(Callable.From(() => {
			if (GodotObject.IsInstanceValid(popup) && popup.IsInsideTree())
			{
				popup.QueueFree();
			}
		}));

		TreeExiting += () => {
			if (GodotObject.IsInstanceValid(popup) && popup.IsInsideTree())
			{
				popup.QueueFree();
			}
		};
	}

	private void Die()
	{
		GD.Print("[Enemy] Враг повержен!");
		DropWeaponRandomly();
		QueueFree();
	}

	private void DropWeaponRandomly()
	{
		if (PossibleWeaponDrops == null || PossibleWeaponDrops.Count == 0) return;

		int randomIndex = GD.RandRange(0, PossibleWeaponDrops.Count - 1);
		string chosenWeaponPath = PossibleWeaponDrops[randomIndex];

		WeaponData weaponData = GD.Load<WeaponData>(chosenWeaponPath);
		if (weaponData == null) return;

		if (WeaponPickupScene != null)
		{
			CallDeferred(nameof(DeferredSpawnPickup), weaponData, GlobalPosition);
		}
		else
		{
			GD.Print($"[Drop] Из врага выпало оружие: {weaponData.ResourceName} ({chosenWeaponPath})");
		}
	}

	private void DeferredSpawnPickup(WeaponData weaponData, Vector2 spawnPosition)
	{
		if (WeaponPickupScene != null)
		{
			var pickup = WeaponPickupScene.Instantiate() as WeaponPickup;
			if (pickup != null)
			{
				GetTree().CurrentScene.AddChild(pickup);
				pickup.Initialize(weaponData, spawnPosition);
			}
		}
	}
}

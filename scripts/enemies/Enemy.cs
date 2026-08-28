using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public int MaxHealth { get; set; } = 30;
	[Export] public float Speed { get; set; } = 80.0f;
	
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public int CurrentHealth { get; private set; }

	private Node2D _player;
	private ProgressBar _healthBar;
	
	private float _damageCooldown = 0.0f;
	private const float AttackCooldownTime = 0.4f; // Кулдаун урона игроку

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		AddToGroup("Enemy"); // Важно для KillZone и других систем

		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		SetupHealthBar();
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

		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

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

		// Дополнительная защита: если враг умирает до завершения анимации, удаляем popup
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
		QueueFree();
	}
}

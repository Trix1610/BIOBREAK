using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public int MaxHealth { get; set; } = 30;
	[Export] public float Speed { get; set; } = 80.0f; // Скорость ходьбы
	
	// Берем значение гравитации из настроек проекта Godot
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public int CurrentHealth { get; private set; }

	private Node2D _player;
	private ProgressBar _healthBar;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		AddToGroup("Enemies");

		// Ищем игрока на сцене по группе "Player"
		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		// Создаем аккуратную полоску здоровья через код
		SetupHealthBar();
	}

	private void SetupHealthBar()
	{
		_healthBar = new ProgressBar();
		_healthBar.MaxValue = MaxHealth;
		_healthBar.Value = CurrentHealth;
		_healthBar.ShowPercentage = false; // Отключаем текст процентов
		
		// Задаем минимальный размер и позицию над головой врага
		_healthBar.CustomMinimumSize = new Vector2(50, 4);
		_healthBar.Position = new Vector2(-15, -24);
		
		// Уменьшаем полоску целиком, чтобы она была компактной и аккуратной
		_healthBar.Scale = new Vector2(0.6f, 0.6f);

		// Красная шкала заполнения
		var styleFill = new StyleBoxFlat();
		styleFill.BgColor = Colors.Red;
		_healthBar.AddThemeStyleboxOverride("fill", styleFill);

		// Темный фон полоски
		var styleBg = new StyleBoxFlat();
		styleBg.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
		_healthBar.AddThemeStyleboxOverride("background", styleBg);

		AddChild(_healthBar);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. Применяем гравитацию, если враг не на полу
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		// 2. Логика преследования игрока по горизонтали (X)
		if (_player != null)
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
	}

	public void TakeDamage(int damage)
	{
		CurrentHealth -= Mathf.Min(damage, CurrentHealth);
		GD.Print($"[Enemy] Получен урон: {damage}. Осталось HP: {CurrentHealth}");

		// Обновляем полоску здоровья
		if (_healthBar != null)
		{
			_healthBar.Value = CurrentHealth;
		}

		// Спавним всплывающий текст с уроном
		SpawnDamagePopup(damage);

		// Вспышка красным цветом при уроне
		Modulate = Colors.Red;
		GetTree().CreateTimer(0.1f).Timeout += () => Modulate = Colors.White;

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
		
		// Добавляем цифру в корень сцены
		GetTree().CurrentScene.AddChild(popup);
		popup.GlobalPosition = GlobalPosition + new Vector2(-10, -35);

		// Создаем твин, привязанный к ДЕРЕВУ СЦЕНЫ, а не к врагу. 
		// Это гарантирует, что анимация доиграет до конца и удалит узел, даже если враг получит урон еще раз или умрет.
		var tween = GetTree().CreateTween().SetParallel(true);
		
		tween.TweenProperty(popup, "global_position", popup.GlobalPosition + new Vector2(0, -15), 0.4f);
		tween.TweenProperty(popup, "modulate:a", 0.0f, 0.4f);

		// Жестко удаляем узел по завершении анимации
		tween.Chain().TweenCallback(Callable.From(() => popup.QueueFree()));
	}

	private void Die()
	{
		GD.Print("[Enemy] Враг повержен!");
		QueueFree();
	}
}

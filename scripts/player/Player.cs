using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float BaseSpeed { get; set; } = 200.0f;
	[Export] public float JumpVelocity { get; set; } = -400.0f; // Сила прыжка (в Godot вверх — это минус)

	// Получаем значение гравитации из настроек проекта Godot
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _jumpsLeft = 0;
	public OrganManager OrganManager { get; private set; }

	public override void _Ready()
	{
		OrganManager = GetNodeOrNull<OrganManager>("OrganManager");
		if (OrganManager == null)
		{
			OrganManager = new OrganManager { Name = "OrganManager" };
			AddChild(OrganManager);
		}
		CallDeferred(nameof(PositionAtSpawnPoint));
	}
	private void PositionAtSpawnPoint()
	{
		if (GameManager.Instance == null) return;

		string targetPointName = GameManager.Instance.TargetSpawnPoint;
		
		// Ищем Marker2D с нужным именем на текущей сцене
		Marker2D spawnPoint = GetParent().FindChild(targetPointName, recursive: true, owned: false) as Marker2D;

		if (spawnPoint != null)
		{
			GlobalPosition = spawnPoint.GlobalPosition;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. ГРАВИТАЦИЯ: Если мы не на земле, прибавляем падение
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
		}
		else
		{
			// На земле сбрасываем счетчик доступных прыжков:
			// 1 базовый прыжок + бонусные прыжки от органов (ExtraJumps)
			_jumpsLeft = 1 + OrganManager.GetTotalExtraJumps();
		}

		// 2. ПРЫЖОК: Нажатие пробела или "ui_accept"
		if (Input.IsActionJustPressed("ui_accept") && _jumpsLeft > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpsLeft--; // Тратим один прыжок
		}

		// 3. ДВИЖЕНИЕ ВЛЕВО/ВПРАВО
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
	public override void _UnhandledInput(InputEvent @event)
	{
		// Нажатие клавиши 'F' на клавиатуре или Клик ЛКМ
		bool isFPressed = @event is InputEventKey k && k.Pressed && !k.Echo && k.Keycode == Key.F;
		bool isMousePressed = @event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left;

		if (isFPressed || isMousePressed)
		{
			Attack();
		}
	}
	private void Attack()
	{
		// Простая проверка врагов поблизости (в радиусе 50 пикселей)
		foreach (Node node in GetParent().GetChildren())
		{
			if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) < 60.0f)
			{
				enemy.TakeDamage(20.0f); // Наносим 20 урона (убивает за 1 удар)
				break;
			}
		}
	}
}

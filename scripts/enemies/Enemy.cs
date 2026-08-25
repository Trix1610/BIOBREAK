using Godot;
using Godot.Collections;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Health { get; set; } = 20.0f;
	[Export] public float Speed { get; set; } = 100.0f; // Скорость бега врага
	[Export] public float DetectionRadius { get; set; } = 400.0f; // Радиус обнаружения игрока

	[Export] public PackedScene DropItemScene { get; set; }
	[Export] public Array<OrganData> PossibleLoot { get; set; } = new();

	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private CharacterBody2D _targetPlayer;

	public override void _Ready()
	{
		// Находим игрока на сцене при старте
		// (Ищем узел в группе "Player" или прямо в родителе)
		_targetPlayer = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. ГРАВИТАЦИЯ
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
		}

		// 2. ДВИЖЕНИЕ К ИГРОКУ (ИИ)
		if (_targetPlayer != null && IsInstanceValid(_targetPlayer))
		{
			float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);

			// Бежим к игроку, только если он в радиусе видимости
			if (distanceToPlayer <= DetectionRadius)
			{
				// Направление: -1 (влево) или +1 (вправо)
				float direction = Mathf.Sign(_targetPlayer.GlobalPosition.X - GlobalPosition.X);
				
				// Устанавливаем горизонтальную скорость
				velocity.X = direction * Speed;
			}
			else
			{
				// Если игрок далеко — останавливаемся
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}
		}
		else
		{
			// Если игрока нет — пытаемся снова его найти
			_targetPlayer = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
			velocity.X = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void TakeDamage(float amount)
	{
		Health -= amount;
		if (Health <= 0) Die();
	}

	private void Die()
	{
		DropLoot();
		QueueFree();
	}

	private void DropLoot()
	{
		if (DropItemScene == null || PossibleLoot.Count == 0) return;

		int randomIndex = GD.RandRange(0, PossibleLoot.Count - 1);
		OrganData randomOrgan = PossibleLoot[randomIndex];

		PickupItem droppedItem = DropItemScene.Instantiate<PickupItem>();
		droppedItem.ItemData = randomOrgan;
		droppedItem.GlobalPosition = GlobalPosition;

		GetParent().AddChild(droppedItem);
	}
}

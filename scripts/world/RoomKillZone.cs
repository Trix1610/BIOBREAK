using Godot;

public partial class RoomKillZone : Node
{
	[Export] public int LimitRight { get; set; } = 1920;
	[Export] public int LimitBottom { get; set; } = 1080;

	private RoomEnemySpawner _enemySpawner;

	public void SetupKillZone(Node2D parent, RoomEnemySpawner enemySpawner)
	{
		_enemySpawner = enemySpawner;

		// Если зона уже есть, не плодим дубликаты
		if (parent.HasNode("KillZone")) return;

		var killZone = new Area2D();
		killZone.Name = "KillZone";

		var collision = new CollisionShape2D();
		var rectShape = new RectangleShape2D();

		// Делаем зону широкой на всю комнату и высокой на 100 пикселей
		float width = LimitRight + 500f;
		rectShape.Size = new Vector2(width, 100f);
		collision.Shape = rectShape;

		// Ставим её по центру по ширине и ниже пола
		collision.Position = new Vector2(LimitRight / 2f, LimitBottom + 100f);

		killZone.AddChild(collision);
		parent.AddChild(killZone);

		// Подписываемся на событие падения
		killZone.BodyEntered += OnKillZoneBodyEntered;
	}

	private void OnKillZoneBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Enemy"))
		{
			GD.Print("[RoomKillZone] Враг упал в зону!");
			body.QueueFree();

			if (_enemySpawner != null)
			{
				_enemySpawner.RemoveEnemy(body);
			}
		}
		else if (body is Player player)
		{
			// Наносим смертельный урон
			player.TakeDamage(999, player.GlobalPosition);
		}
	}
}

using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene { get; set; }
	
	// Границы спавна по оси X (например, от 100 до 700 по ширине платформы)
	[Export] public float MinX { get; set; } = 100.0f;
	[Export] public float MaxX { get; set; } = 700.0f;
	
	// Высота спавна над платформой (по оси Y)
	[Export] public float SpawnY { get; set; } = 50.0f;
	
	// Интервал спавна в секундах
	[Export] public float SpawnInterval { get; set; } = 3.0f;

	private Timer _timer;

	public override void _Ready()
	{
		// Создаем таймер программно
		_timer = new Timer();
		_timer.WaitTime = SpawnInterval;
		_timer.Autostart = true;
		_timer.Timeout += SpawnEnemy;
		AddChild(_timer);
	}

	private void SpawnEnemy()
	{
		if (EnemyScene == null) return;

		// Генерируем случайную X-координату
		float randomX = (float)GD.RandRange(MinX, MaxX);

		// Создаем врага и ставим его в воздухе
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = new Vector2(randomX, SpawnY);

		// Добавляем врага на главную сцену
		GetParent().AddChild(enemy);
	}
}

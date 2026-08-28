using Godot;

public partial class Bullet : Area2D
{
	[Export] public float Speed { get; set; } = 500.0f;
	[Export] public int Damage { get; set; } = 20;
	
	private Vector2 _direction = Vector2.Zero;

	public override void _Ready()
	{
		// Программно подписываемся на событие столкновения
		BodyEntered += OnBodyEntered;
	}

	public void Initialize(Vector2 direction)
	{
		_direction = direction.Normalized();
		Rotation = _direction.Angle();
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += _direction * Speed * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Проверяем, во что врезались
		if (body is Enemy enemy)
		{
			enemy.TakeDamage(Damage);
			QueueFree(); // Уничтожаем пулю при попадании во врага
		}
		else if (body is not Player) // Игнорируем самого игрока, чтобы пуля не взрывалась при выстреле
		{
			// Если врезались в стены, платформы или другие препятствия
			QueueFree();
		}
	}
}

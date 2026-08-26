using Godot;

public partial class HealthUI : CanvasLayer
{
	[Export] public int MaxHearts { get; set; } = 3;
	private HBoxContainer _container;

	public override void _Ready()
	{
		_container = GetNode<HBoxContainer>("HBoxContainer");
		
		// Автоматически задаем отступ от верхнего левого угла экрана (X: 20, Y: 20)
		_container.Position = new Vector2(20, 20);
		
		// Устанавливаем расстояние (в пикселях) между сердечками
		_container.AddThemeConstantOverride("separation", 8);

		UpdateHearts(MaxHearts);
	}

	// Метод для обновления отображения сердечек (принимает текущее кол-во жизней)
	public void UpdateHearts(int currentHealth)
	{
		if (_container == null) return;

		// Удаляем старые сердечки перед отрисовкой новых
		foreach (Node child in _container.GetChildren())
		{
			child.QueueFree();
		}

		// Создаем новые сердечки в зависимости от текущего здоровья
		for (int i = 0; i < currentHealth; i++)
		{
			var heart = new Label();
			heart.Text = "❤️";
			heart.AddThemeFontSizeOverride("font_size", 20);
			_container.AddChild(heart);
		}
	}
}

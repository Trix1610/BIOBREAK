using Godot;

public partial class PickupItem : Area2D
{
	// Ссылка на конкретный файл ресурса (.tres), назначается в Инспекторе
	[Export] public OrganData ItemData { get; set; }

	public override void _Ready()
	{
		// Подключаем сигнал входа объекта в зону триггера
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Проверяем, что в триггер вошёл именно Player
		if (body is Player player)
		{
			if (ItemData != null)
			{
				// Передаём данные органа в OrganManager игрока
				player.OrganManager.AddOrgan(ItemData);
				
				// Удаляем предмет с карты
				QueueFree(); 
			}
			else
			{
				GD.PrintErr($"ОШИБКА: У объекта {Name} в Инспекторе не назначен файл ItemData (.tres)!");
			}
		}
	}
}

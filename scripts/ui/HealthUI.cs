using Godot;

public partial class HealthUI : CanvasLayer
{
	public static HealthUI Instance { get; private set; }

	[Export] public Texture2D FullHeartTexture { get; set; }
	[Export] public Texture2D HalfHeartTexture { get; set; }
	[Export] public Texture2D EmptyHeartTexture { get; set; }

	private HBoxContainer _container;
	private Player _player;
	
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}

		ProcessMode = ProcessModeEnum.Always;

		_container = GetNodeOrNull<HBoxContainer>("HBoxContainer");
		if (_container == null)
		{
			GD.PrintErr("[HealthUI] ОШИБКА: Не найден узел HBoxContainer!");
		}
	}

	public void SetupForPlayer(Player newPlayer)
	{
		if (newPlayer == null) return;
		
		_player = newPlayer;
		GD.Print($"[HealthUI] Настройка для игрока -> Max: {_player.MaxHealth}, Current: {_player.CurrentHealth}");

		CreateHearts(_player.MaxHealth);
		UpdateHealth(_player.CurrentHealth);
	}

	public void CreateHearts(int maxHealth)
	{
		if (_container == null) return;

		// МГНОВЕННО удаляем старые узлы, чтобы они сразу исчезли из контейнера
		foreach (Node child in _container.GetChildren())
		{
			child.Free();
		}

		int heartCount = maxHealth / 2;
		for (int i = 0; i < heartCount; i++)
		{
			var heartRect = new TextureRect();
			heartRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			heartRect.CustomMinimumSize = new Vector2(32, 32); 
			_container.AddChild(heartRect);
		}
		GD.Print($"[HealthUI] Создано новых сердец: {heartCount}");
	}

	public void UpdateHealth(int currentHealth)
	{
		if (_container == null) return;

		if (_player == null || !IsInstanceValid(_player))
		{
			_player = GetTree()?.GetFirstNodeInGroup("Player") as Player;
			if (_player == null) return;
		}

		var hearts = _container.GetChildren();

		for (int i = 0; i < hearts.Count; i++)
		{
			if (hearts[i] is TextureRect heartRect)
			{
				int heartIndexValue = currentHealth - (i * 2);

				if (heartIndexValue >= 2)
				{
					heartRect.Texture = FullHeartTexture;
					heartRect.Visible = true;
				}
				else if (heartIndexValue == 1)
				{
					heartRect.Texture = HalfHeartTexture;
					heartRect.Visible = true;
				}
				else
				{
					if (EmptyHeartTexture != null)
					{
						heartRect.Texture = EmptyHeartTexture;
						heartRect.Visible = true;
					}
					else
					{
						heartRect.Visible = false;
					}
				}
			}
		}
		GD.Print($"[HealthUI] Здоровье успешно отрисовано: {currentHealth}");
	}
}

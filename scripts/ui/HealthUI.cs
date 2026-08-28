using Godot;

public partial class HealthUI : CanvasLayer
{
	// Синглтон для удобного доступа из любой точки игры (например, из Player.cs)
	public static HealthUI Instance { get; private set; }

	[Export] public Texture2D FullHeartTexture { get; set; }
	[Export] public Texture2D HalfHeartTexture { get; set; }
	[Export] public Texture2D EmptyHeartTexture { get; set; }

	private HBoxContainer _container;
	private Player _player;

	public override void _Ready()
	{
		// Настраиваем синглтон
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree(); // Защита от создания дубликатов
			return;
		}

		_container = GetNodeOrNull<HBoxContainer>("HBoxContainer");
		if (_container == null)
		{
			GD.PrintErr("HealthUI: Не найден узел HBoxContainer!");
			return;
		}

		// Инициализируем интерфейс на следующем кадре, когда игрок успеет заспавниться
		CallDeferred(nameof(TryFindPlayerAndInit));
	}

	private void TryFindPlayerAndInit()
	{
		_player = GetTree()?.GetFirstNodeInGroup("Player") as Player;
		
		if (_player != null)
		{
			CreateHearts(_player.MaxHealth);
			UpdateHealth(_player.CurrentHealth);
		}
		else
		{
			// Если игрок еще не появился, пробуем найти его чуть позже
			_ = DelayedFindPlayer();
		}
	}

	private async System.Threading.Tasks.Task DelayedFindPlayer()
	{
		int attempts = 0;
		while (_player == null && attempts < 30)
		{
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			if (!IsInsideTree() || GetTree() == null) return;
			
			_player = GetTree().GetFirstNodeInGroup("Player") as Player;
		}

		if (_player != null)
		{
			CreateHearts(_player.MaxHealth);
			UpdateHealth(_player.CurrentHealth);
		}
		else
		{
			GD.PrintErr("HealthUI: Не удалось найти игрока в группе 'Player' после ожидания!");
		}
	}

	public void CreateHearts(int maxHealth)
	{
		if (_container == null) return;

		foreach (Node child in _container.GetChildren())
		{
			child.QueueFree();
		}

		// Каждое сердце равно 2 единицам здоровья
		int heartCount = maxHealth / 2;
		for (int i = 0; i < heartCount; i++)
		{
			var heartRect = new TextureRect();
			heartRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			_container.AddChild(heartRect);
		}
	}

	public void UpdateHealth(int currentHealth)
	{
		if (_container == null) return;

		// Если игрок не был привязан ранее, пробуем найти его на лету
		if (_player == null)
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
	}
}

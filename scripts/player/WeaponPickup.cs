using Godot;

public partial class WeaponPickup : Area2D
{
	[Export] public WeaponData WeaponDataToDrop { get; set; }

	private Vector2 _velocity;
	private float _gravity = 980.0f;
	private bool _isLanded = false;
	
	private bool _playerInRange = false;
	private Player _currentPlayer = null;
	private Label _promptLabel;

	public override void _Ready()
	{
		// Подписываемся на вход и выход игрока из зоны
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		_promptLabel = GetNodeOrNull<Label>("PromptLabel");
		if (_promptLabel != null)
		{
			_promptLabel.Visible = false; // Скрываем текст по умолчанию
		}
	}

	public void Initialize(WeaponData data, Vector2 spawnPosition)
	{
		WeaponDataToDrop = data;
		GlobalPosition = spawnPosition;

		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (sprite != null && data.WeaponSprite != null)
		{
			sprite.Texture = data.WeaponSprite;
		}

		// Устанавливаем текст подсказки, если имя задано в ресурсе
		if (_promptLabel != null && data != null)
		{
			_promptLabel.Text = $"{data.WeaponName}";
		}

		// Начальный импульс вылета
		float randomX = (float)GD.RandRange(-120, 120);
		float randomY = (float)GD.RandRange(-220, -140);
		_velocity = new Vector2(randomX, randomY);
	}

	public override void _PhysicsProcess(double delta)
	{
		// Логика падения на пол
		if (!_isLanded)
		{
			_velocity.Y += _gravity * (float)delta;
			GlobalPosition += _velocity * (float)delta;
		}
	}

	// Перехватываем нажатие клавиши E, когда игрок рядом
	public override void _Input(InputEvent @event)
	{
		if (_playerInRange && _currentPlayer != null)
		{
			// Проверяем нажатие клавиши E
			if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.E)
			{
				PickUpWeapon(_currentPlayer);
			}
		}
	}

	private void PickUpWeapon(Player player)
	{
		if (WeaponDataToDrop != null)
		{
			player.EquipWeapon(WeaponDataToDrop);
			GD.Print($"[Pickup] Игрок сменил оружие на: {WeaponDataToDrop.WeaponName}");
		}
		
		// Уничтожаем объект с пола после подбора
		QueueFree();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			_playerInRange = true;
			_currentPlayer = player;

			if (_promptLabel != null)
			{
				_promptLabel.Visible = true; // Показываем надпись над пушкой
			}
		}
		else if (!_isLanded && body.Name == "Environment")
		{
			_isLanded = true;
			_velocity = Vector2.Zero;
			GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - 12); 
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body == _currentPlayer)
		{
			_playerInRange = false;
			_currentPlayer = null;

			if (_promptLabel != null)
			{
				_promptLabel.Visible = false; // Скрываем надпись, когда игрок отошел
			}
		}
	}
}

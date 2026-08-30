using Godot;

public abstract partial class WeaponBase : Node2D
{
	[Export] public WeaponData Data { get; set; }
	
	protected float _fireCooldown = 0.0f;
	protected Sprite2D _sprite;

	public override void _Ready()
	{
		// ===== ИЗМЕНЕНО: создаём Sprite2D программно, если его нет =====
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		
		if (_sprite == null)
		{
			// Если Sprite2D не найден, создаём его
			_sprite = new Sprite2D();
			AddChild(_sprite);
			GD.Print("[WeaponBase] Создан Sprite2D для оружия");
		}
		
		// Устанавливаем текстуру из данных
		if (Data != null && Data.WeaponSprite != null)
		{
			_sprite.Texture = Data.WeaponSprite;
		}
		else
		{
			GD.PrintErr("[WeaponBase] Нет текстуры для оружия!");
		}
		
		// ===== НАСТРОЙКА ПОЗИЦИИ И МАСШТАБА ОРУЖИЯ =====
		// Позиция относительно игрока (рука)
		_sprite.Position = new Vector2(15, -5);
		
		// ===== МАСШТАБ ИЗ DATA =====
		float scale = 1.0f;
		if (Data != null)
		{
			scale = Data.WeaponScale; // Используем параметр из WeaponData
		}
		_sprite.Scale = new Vector2(scale, scale);
		
		// Центрируем спрайт (чтобы вращался вокруг центра)
		_sprite.Centered = true;
		
		GD.Print($"[WeaponBase] Оружие загружено: {Data?.WeaponName ?? "Unknown"}, масштаб: {scale}");
	}

	public override void _Process(double delta)
	{
		if (_fireCooldown > 0)
			_fireCooldown -= (float)delta;
	}

	public abstract void Shoot(Vector2 spawnPosition, Vector2 targetPosition);
}

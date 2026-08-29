using Godot;

public abstract partial class WeaponBase : Node2D
{
	[Export] public WeaponData Data { get; set; }
	
	protected float _fireCooldown = 0.0f;
	protected Sprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (Data != null && Data.WeaponSprite != null && _sprite != null)
		{
			_sprite.Texture = Data.WeaponSprite;
		}
	}

	public override void _Process(double delta)
	{
		if (_fireCooldown > 0)
			_fireCooldown -= (float)delta;
	}

	public abstract void Shoot(Vector2 spawnPosition, Vector2 targetPosition);
}

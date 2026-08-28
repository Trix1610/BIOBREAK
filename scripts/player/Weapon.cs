using Godot;

public partial class Weapon : Node2D
{
	[Export] public WeaponData Data { get; set; }
	
	private float _fireCooldown = 0.0f;
	private Sprite2D _sprite;

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

	public void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		PackedScene bulletScene = Data.BulletScene ?? GD.Load<PackedScene>("res://scenes/player/Bullet.tscn");
		if (bulletScene == null) return;

		if (bulletScene.Instantiate() is Bullet bullet)
		{
			bullet.Speed = Data.BulletSpeed;
			bullet.Damage = Data.Damage;

			GetTree().Root.AddChild(bullet);
			bullet.GlobalPosition = spawnPosition;

			Vector2 direction = (targetPosition - spawnPosition);
			bullet.Initialize(direction);

			_fireCooldown = Data.FireRate;
		}
	}
}

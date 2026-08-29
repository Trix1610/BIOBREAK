using Godot;

public partial class ExplosiveWeapon : AreaWeapon
{
	[Export] public float ExplosionDelay { get; set; } = 0.5f;

	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		Vector2 direction = (targetPosition - spawnPosition).Normalized();
		float range = Data.Range > 0 ? Data.Range : 800f;

		// Создаем снаряд (можно использовать Bullet с задержкой)
		PackedScene bulletScene = Data.BulletScene ?? GD.Load<PackedScene>("res://scenes/player/Bullet.tscn");
		if (bulletScene == null) return;

		if (bulletScene.Instantiate() is Bullet bullet)
		{
			bullet.Speed = Data.BulletSpeed;
			bullet.Damage = Data.Damage;

			GetTree().Root.AddChild(bullet);
			bullet.GlobalPosition = spawnPosition;

			bullet.Initialize(direction);

			// Устанавливаем задержку взрыва
			var timer = GetTree().CreateTimer(ExplosionDelay);
			timer.Timeout += () => 
			{
				if (IsInstanceValid(bullet))
				{
					DealAreaDamage(bullet.GlobalPosition, Data.AreaRadius > 0 ? Data.AreaRadius : 150f, penetrateWalls: false);
					bullet.QueueFree();
				}
			};

			_fireCooldown = Data.FireRate;
		}
	}
}

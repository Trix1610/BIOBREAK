using Godot;

public abstract partial class ProjectileWeapon : WeaponBase
{
	protected void SpawnBullet(Vector2 spawnPosition, Vector2 targetPosition, float spreadAngle = 0f)
	{
		if (Data == null) return;

		PackedScene bulletScene = Data.BulletScene ?? GD.Load<PackedScene>("res://scenes/player/Bullet.tscn");
		if (bulletScene == null) return;

		if (bulletScene.Instantiate() is Bullet bullet)
		{
			bullet.Speed = Data.BulletSpeed;
			bullet.Damage = Data.Damage;

			GetTree().Root.AddChild(bullet);
			bullet.GlobalPosition = spawnPosition;

			Vector2 direction = (targetPosition - spawnPosition).Normalized();
			
			if (spreadAngle != 0f)
			{
				direction = direction.Rotated(Mathf.DegToRad(spreadAngle));
			}
			
			bullet.Initialize(direction);

			_fireCooldown = Data.FireRate;
		}
	}
}

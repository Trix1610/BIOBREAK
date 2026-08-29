using Godot;

public partial class SingleShotWeapon : ProjectileWeapon
{
	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		SpawnBullet(spawnPosition, targetPosition, 0f);
	}
}

using Godot;

public partial class LaserWeapon : RaycastWeapon
{
	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		FireRaycast(spawnPosition, targetPosition);
	}
}

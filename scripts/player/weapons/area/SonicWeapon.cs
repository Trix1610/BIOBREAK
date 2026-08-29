using Godot;

public partial class SonicWeapon : AreaWeapon
{
	[Export] public bool UseCone { get; set; } = false;
	[Export] public float ConeAngle { get; set; } = 90f;

	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		Vector2 direction = (targetPosition - spawnPosition).Normalized();
		float range = Data.Range > 0 ? Data.Range : 500f;
		float radius = Data.AreaRadius > 0 ? Data.AreaRadius : 200f;

		if (UseCone)
		{
			float angle = ConeAngle;
			DealConeDamage(spawnPosition, direction, range, angle, penetrateWalls: false);
		}
		else
		{
			DealAreaDamage(spawnPosition, radius, penetrateWalls: false);
		}
	}
}

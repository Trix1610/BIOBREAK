using Godot;

public partial class RailgunWeapon : RaycastWeapon
{
	[Export] public bool CanPenetrate { get; set; } = true;
	[Export] public int MaxPenetrations { get; set; } = 3;

	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		if (CanPenetrate)
		{
			FirePenetratingRaycast(spawnPosition, targetPosition);
		}
		else
		{
			FireRaycast(spawnPosition, targetPosition);
		}
	}

	private void FirePenetratingRaycast(Vector2 spawnPosition, Vector2 targetPosition)
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		Vector2 direction = (targetPosition - spawnPosition).Normalized();
		float range = Data.Range > 0 ? Data.Range : 1000f;

		Vector2 currentPos = spawnPosition;
		int penetrations = 0;

		while (penetrations < MaxPenetrations)
		{
			var query = PhysicsRayQueryParameters2D.Create(
				currentPos,
				currentPos + direction * range,
				collisionMask: 1
			);

			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				var collider = result["collider"].AsGodotObject();
				Vector2 hitPoint = result["position"].AsVector2();

				if (collider is Enemy enemy)
				{
					enemy.TakeDamage(Data.Damage);
					penetrations++;
				}
				else
				{
					// Попали в стену - останавливаемся
					CreateRayVisual(spawnPosition, hitPoint);
					return;
				}

				currentPos = hitPoint + direction * 10f; // Сдвигаемся немного вперед
			}
			else
			{
				// Не попали ни во что
				CreateRayVisual(spawnPosition, currentPos + direction * range);
				return;
			}
		}

		_fireCooldown = Data.FireRate;
	}
}

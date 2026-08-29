using Godot;

public abstract partial class RaycastWeapon : WeaponBase
{
	protected void FireRaycast(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null) return;

		var spaceState = GetWorld2D().DirectSpaceState;
		Vector2 direction = (targetPosition - spawnPosition).Normalized();
		float range = Data.Range > 0 ? Data.Range : 1000f;

		var query = PhysicsRayQueryParameters2D.Create(
			spawnPosition,
			spawnPosition + direction * range,
			collisionMask: 1
		);

		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			var collider = result["collider"].AsGodotObject();
			if (collider is Enemy enemy)
			{
				enemy.TakeDamage(Data.Damage);
			}

			// Визуализация луча (можно добавить трейсер)
			Vector2 hitPoint = result["position"].AsVector2();
			CreateRayVisual(spawnPosition, hitPoint);
		}
		else
		{
			// Луч не попал ни во что - рисуем до конца дальности
			CreateRayVisual(spawnPosition, spawnPosition + direction * range);
		}

		_fireCooldown = Data.FireRate;
	}

	protected void CreateRayVisual(Vector2 start, Vector2 end)
	{
		var line = new Line2D();
		line.AddPoint(start);
		line.AddPoint(end);
		line.Width = 2;
		line.DefaultColor = new Color(1, 0, 0, 0.8f);
		GetTree().Root.AddChild(line);

		// Удаляем линию через 0.1 секунды
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => 
		{
			if (IsInstanceValid(line)) line.QueueFree();
		};
	}
}

using Godot;
using System.Collections.Generic;

public abstract partial class AreaWeapon : WeaponBase
{
	protected void DealAreaDamage(Vector2 centerPosition, float radius, bool penetrateWalls = false)
	{
		if (Data == null) return;

		var enemies = GetTree().GetNodesInGroup("Enemy");
		int enemiesHit = 0;

		foreach (Node node in enemies)
		{
			if (node is Enemy enemy && IsInstanceValid(enemy))
			{
				float distance = centerPosition.DistanceTo(enemy.GlobalPosition);
				
				if (distance <= radius)
				{
					// Проверка на стены если нужно
					if (!penetrateWalls && IsBlockedByWall(centerPosition, enemy.GlobalPosition))
					{
						continue;
					}

					// Уменьшаем урон с расстоянием
					float falloff = 1f - (distance / radius);
					int damage = (int)(Data.Damage * falloff);
					
					enemy.TakeDamage(damage);
					enemiesHit++;
				}
			}
		}

		// Визуализация области
		CreateAreaVisual(centerPosition, radius);

		_fireCooldown = Data.FireRate;
	}

	protected void DealConeDamage(Vector2 origin, Vector2 direction, float range, float angle, bool penetrateWalls = false)
	{
		if (Data == null) return;

		var enemies = GetTree().GetNodesInGroup("Enemy");
		int enemiesHit = 0;

		foreach (Node node in enemies)
		{
			if (node is Enemy enemy && IsInstanceValid(enemy))
			{
				Vector2 toEnemy = enemy.GlobalPosition - origin;
				float distance = toEnemy.Length();
				
				if (distance <= range)
				{
					Vector2 enemyDir = toEnemy.Normalized();
					float angleToEnemy = direction.AngleTo(enemyDir);
					float halfAngle = angle / 2f;

					if (Mathf.Abs(angleToEnemy) <= halfAngle)
					{
						if (!penetrateWalls && IsBlockedByWall(origin, enemy.GlobalPosition))
						{
							continue;
						}

						float falloff = 1f - (distance / range);
						int damage = (int)(Data.Damage * falloff);
						
						enemy.TakeDamage(damage);
						enemiesHit++;
					}
				}
			}
		}

		// Визуализация конуса
		CreateConeVisual(origin, direction, range, angle);

		_fireCooldown = Data.FireRate;
	}

	private bool IsBlockedByWall(Vector2 from, Vector2 to)
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(from, to, collisionMask: 2);
		var result = spaceState.IntersectRay(query);
		return result.Count > 0;
	}

	private void CreateAreaVisual(Vector2 center, float radius)
	{
		var circle = new ColorRect();
		circle.Position = center - new Vector2(radius, radius);
		circle.Size = new Vector2(radius * 2, radius * 2);
		circle.Color = new Color(1, 1, 0, 0.3f);
		circle.Rotation = 0;
		GetTree().Root.AddChild(circle);

		var timer = GetTree().CreateTimer(0.15f);
		timer.Timeout += () => 
		{
			if (IsInstanceValid(circle)) circle.QueueFree();
		};
	}

	private void CreateConeVisual(Vector2 origin, Vector2 direction, float range, float angle)
	{
		var line = new Line2D();
		line.AddPoint(origin);
		
		float halfAngle = angle / 2f;
		Vector2 leftDir = direction.Rotated(-halfAngle);
		Vector2 rightDir = direction.Rotated(halfAngle);
		
		line.AddPoint(origin + leftDir * range);
		line.AddPoint(origin + rightDir * range);
		line.AddPoint(origin);
		
		line.Width = 2;
		line.DefaultColor = new Color(1, 1, 0, 0.5f);
		line.Closed = true;
		GetTree().Root.AddChild(line);

		var timer = GetTree().CreateTimer(0.15f);
		timer.Timeout += () => 
		{
			if (IsInstanceValid(line)) line.QueueFree();
		};
	}
}

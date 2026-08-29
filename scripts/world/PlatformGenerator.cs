using Godot;
using System.Collections.Generic;

public partial class PlatformGenerator : Node
{
	[Export] public int LimitRight { get; set; } = 1920;

	public void GeneratePlatforms(Node2D parent, Vector2I roomGridPos)
	{
		if (parent.HasNode("AutoPlatforms")) return;

		var platformRoot = new Node2D();
		platformRoot.Name = "AutoPlatforms";
		parent.AddChild(platformRoot);

		// Уникальный сид для каждой комнаты
		int roomSeed = 1337;
		if (RunManager.Instance?.CurrentRoom != null)
		{
			var pos = roomGridPos;
			roomSeed = Mathf.Abs(pos.X * 73856093 ^ pos.Y * 19349663);
		}

		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong)roomSeed;

		float usableWidth = LimitRight - 200f;
		int platformCount = LimitRight > 2000 ? rng.RandiRange(10, 14) : rng.RandiRange(5, 8);

		List<Vector2> spawnedPositions = new();

		// НИЗКИЕ И БЛИЗКИЕ ЯРУСЫ: опускаем всё ближе к полу и уменьшаем шаги по высоте
		float[] heightTiers = { 920f, 780f, 640f };

		foreach (var tierY in heightTiers)
		{
			int perTier = rng.RandiRange(1, LimitRight > 2000 ? 4 : 2);
			float segmentWidth = usableWidth / perTier;

			for (int i = 0; i < perTier; i++)
			{
				float minX = 150f + (i * segmentWidth);
				float maxX = minX + segmentWidth - 100f;

				if (maxX <= minX) continue;

				float randomX = rng.RandfRange(minX, maxX);
				float randomY = tierY + rng.RandfRange(-20f, 20f);

				Vector2 newPos = new Vector2(randomX, randomY);

				bool tooClose = false;
				foreach (var existingPos in spawnedPositions)
				{
					if (newPos.DistanceTo(existingPos) < 180f)
					{
						tooClose = true;
						break;
					}
				}

				if (tooClose) continue;
				spawnedPositions.Add(newPos);

				float width = rng.RandfRange(250f, 400f);
				Vector2 size = new Vector2(width, 24f);

				var staticBody = new StaticBody2D();
				staticBody.Position = newPos;

				var collision = new CollisionShape2D();
				var rectShape = new RectangleShape2D();
				rectShape.Size = size;
				collision.Shape = rectShape;
				staticBody.AddChild(collision);

				var colorRect = new ColorRect();
				colorRect.Size = size;
				colorRect.Position = -size / 2f;
				colorRect.Color = new Color(0.18f, 0.2f, 0.28f);

				var borderRect = new ColorRect();
				borderRect.Size = new Vector2(size.X, 4);
				borderRect.Position = new Vector2(-size.X / 2f, -size.Y / 2f);
				borderRect.Color = new Color(0.4f, 0.6f, 0.9f);

				staticBody.AddChild(colorRect);
				staticBody.AddChild(borderRect);

				platformRoot.AddChild(staticBody);
			}
		}
	}
}

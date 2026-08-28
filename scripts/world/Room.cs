using Godot;
using System.Collections.Generic;
using System;

public partial class Room : Node2D
{
	[Export] public bool IsLeftDoorClosed { get; set; } = false;
	[Export] public bool IsRightDoorClosed { get; set; } = false;

	[Export] public int LimitLeft { get; set; } = 0;
	[Export] public int LimitTop { get; set; } = 0;
	[Export] public int LimitRight { get; set; } = 1920; 
	[Export] public int LimitBottom { get; set; } = 1080;

	[Export] public PackedScene EnemyScene { get; set; } = GD.Load<PackedScene>("res://scenes/enemies/Enemy.tscn");

	private PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
	private bool _isTransitioning = false;
	private bool _isBattleActive = false;

	private List<Node> _activeEnemies = new List<Node>();
	private Area2D _leftDoor;
	private Area2D _rightDoor;

	public async override void _Ready()
	{
		try
		{
			GenerateProceduralPlatforms();
			SetupDoorsAndSpawns();
			SetupKillZone();

			Node2D player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
			if (player == null && _playerScene != null)
			{
				player = _playerScene.Instantiate<Node2D>();
				AddChild(player);
			}

			string targetSpawnName = GameManager.Instance?.TargetSpawnPoint;
			if (!string.IsNullOrEmpty(targetSpawnName))
			{
				Marker2D spawnPoint = GetNodeOrNull<Marker2D>(targetSpawnName);
				if (spawnPoint != null && player != null)
				{
					player.GlobalPosition = spawnPoint.GlobalPosition;
				}
			}

			var camera = player?.GetNodeOrNull<Camera2D>("Camera2D");
			if (camera != null)
			{
				camera.LimitLeft = LimitLeft;
				camera.LimitTop = LimitTop;
				camera.LimitRight = LimitRight;
				camera.LimitBottom = LimitBottom;

				camera.LimitSmoothed = true;
				camera.PositionSmoothingEnabled = true;
				camera.DragHorizontalEnabled = false;
				camera.DragVerticalEnabled = false;

				camera.ResetSmoothing();
				camera.GlobalPosition = player.GlobalPosition;
			}

			_leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
			_rightDoor = GetNodeOrNull<Area2D>("RightDoor");

			SetupDoors();
			CheckRoomClearance();

			// ЗАЩИТА ОТ ДВОЙНОГО СРАБАТЫВАНИЯ:
			// Временно выключаем двери при входе в комнату, чтобы игрок успел отойти от спавна
			if (_leftDoor != null) _leftDoor.Monitoring = false;
			if (_rightDoor != null) _rightDoor.Monitoring = false;

			// Ждем 0.3 секунды, давая игроку время сойти с точки спавна/двери
			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

			// Включаем обратно только те двери, которые должны работать и если нет активного боя
			if (_leftDoor != null && !IsLeftDoorClosed && !_isBattleActive)
				_leftDoor.Monitoring = true;
			if (_rightDoor != null && !IsRightDoorClosed && !_isBattleActive)
				_rightDoor.Monitoring = true;
		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[Room] Ошибка в _Ready: {ex.Message}");
		}
	}

	private void GenerateProceduralPlatforms()
	{
		if (HasNode("AutoPlatforms")) return;

		var platformRoot = new Node2D();
		platformRoot.Name = "AutoPlatforms";
		AddChild(platformRoot);

		// Уникальный сид для каждой комнаты
		int roomSeed = 1337;
		if (RunManager.Instance?.CurrentRoom != null)
		{
			var pos = RunManager.Instance.CurrentRoom.GridPos;
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
	
	private void SetupKillZone()
	{
		// Если зона уже есть (например, вручную создали), не плодим дубликаты
		if (HasNode("KillZone")) return;

		var killZone = new Area2D();
		killZone.Name = "KillZone";
		
		var collision = new CollisionShape2D();
		var rectShape = new RectangleShape2D();
		
		// Делаем зону широкой на всю комнату и высокой на 100 пикселей,
		// располагаем её ниже нижней границы экрана (например, на LimitBottom + 100)
		float width = LimitRight + 500f; // С запасом по бокам
		rectShape.Size = new Vector2(width, 100f);
		collision.Shape = rectShape;
		
		// Ставим её по центру по ширине и ниже пола
		collision.Position = new Vector2(LimitRight / 2f, LimitBottom + 100f);
		
		killZone.AddChild(collision);
		AddChild(killZone);

		// Подписываемся на событие падения
		killZone.BodyEntered += OnKillZoneBodyEntered;
	}

	private void OnKillZoneBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Enemy"))
		{
			GD.Print($"[KillZone] Враг упал в зону! Всего врагов до удаления: {_activeEnemies.Count}");
			body.QueueFree();
			
			// Принудительно убираем из списка прямо сейчас, не дожидаясь кадров
			_activeEnemies.Remove(body);
			
			GD.Print($"[KillZone] Осталось в списке после ручного удаления: {_activeEnemies.Count}");
			
			if (_activeEnemies.Count == 0 && _isBattleActive)
			{
				GD.Print("[KillZone] Все враги повержены через KillZone! Открываем двери.");
				UnlockDoors();
			}
		}
		else if (body is Player player)
		{
			// Наносим смертельный урон или сразу вызываем смерть
			player.TakeDamage(999, player.GlobalPosition); 
		}
	}

	private async void DelayedCheckEnemies()
	{
		// Ждем один кадр, чтобы QueueFree() точно завершился
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		CheckEnemiesCount();
	}
	
	private void SetupDoorsAndSpawns()
	{
		float rightWallX = LimitRight; 

		// --- ПРАВАЯ СТОРОНА ---
		var rightDoor = GetNodeOrNull<Area2D>("RightDoor");
		if (rightDoor != null)
		{
			var pos = rightDoor.GlobalPosition;
			pos.X = rightWallX - 40; // Дверь остается у стены
			rightDoor.GlobalPosition = pos;
		}

		var spawnRight = GetNodeOrNull<Marker2D>("SpawnRight");
		if (spawnRight != null)
		{
			var pos = spawnRight.GlobalPosition;
			// Было 60px от стены, стало 100px (на 40px дальше вглубь комнаты)
			pos.X = rightWallX - 100; 
			spawnRight.GlobalPosition = pos;
		}

		// --- ЛЕВАЯ СТОРОНА ---
		var leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
		if (leftDoor != null)
		{
			var pos = leftDoor.GlobalPosition;
			pos.X = 40; // Дверь остается у стены
			leftDoor.GlobalPosition = pos;
		}

		var spawnLeft = GetNodeOrNull<Marker2D>("SpawnLeft");
		if (spawnLeft != null)
		{
			var pos = spawnLeft.GlobalPosition;
			// Было 60px от стены, стало 100px (на 40px дальше вглубь комнаты)
			pos.X = 100;
			spawnLeft.GlobalPosition = pos;
		}
	}

	private void CheckRoomClearance()
	{
		var currentRoomNode = RunManager.Instance?.CurrentRoom;
		
		if (currentRoomNode != null && !currentRoomNode.IsCleared)
		{
			SpawnEnemies();

			if (_activeEnemies.Count > 0)
			{
				LockDoors();
			}
			else
			{
				currentRoomNode.IsCleared = true;
			}
		}
	}

	private void SpawnEnemies()
	{
		if (EnemyScene == null) return;

		_activeEnemies.Clear();
		Vector2 screenSize = GetViewportRect().Size;
		var random = new RandomNumberGenerator();
		random.Randomize();

		int enemiesToSpawn = 3;

		for (int i = 0; i < enemiesToSpawn; i++)
		{
			Node2D enemyInstance = EnemyScene.Instantiate<Node2D>();
			AddChild(enemyInstance);

			enemyInstance.TreeExited += OnEnemyDefeated;
			_activeEnemies.Add(enemyInstance);

			float randomX = random.RandfRange(150.0f, screenSize.X - 150.0f);
			float randomY = random.RandfRange(150.0f, screenSize.Y - 150.0f);

			enemyInstance.GlobalPosition = new Vector2(randomX, randomY);
		}
	}

	private void LockDoors()
	{
		_isBattleActive = true;
		if (_leftDoor != null) _leftDoor.Monitoring = false;
		if (_rightDoor != null) _rightDoor.Monitoring = false;
	}

	private void UnlockDoors()
	{
		_isBattleActive = false;
		if (RunManager.Instance?.CurrentRoom != null)
		{
			RunManager.Instance.CurrentRoom.IsCleared = true;
		}

		if (_leftDoor != null && !IsLeftDoorClosed)
		{
			_leftDoor.Monitoring = true;
			CheckDoorOverlap(_leftDoor, OnLeftDoorEntered);
		}

		if (_rightDoor != null && !IsRightDoorClosed)
		{
			_rightDoor.Monitoring = true;
			CheckDoorOverlap(_rightDoor, OnRightDoorEntered);
		}
	}

	private void CheckDoorOverlap(Area2D door, System.Action<Node2D> onEntered)
	{
		var bodies = door.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body.IsInGroup("Player"))
			{
				onEntered(body);
				break;
			}
		}
	}

	private void OnEnemyDefeated()
	{
		CallDeferred(nameof(CheckEnemiesCount));
	}

	private void CheckEnemiesCount()
	{
		_activeEnemies.RemoveAll(e => !GodotObject.IsInstanceValid(e) || !e.IsInsideTree());

		GD.Print($"[CheckEnemiesCount] Активных врагов: {_activeEnemies.Count}, Бой активен: {_isBattleActive}");

		if (_activeEnemies.Count == 0 && _isBattleActive)
		{
			UnlockDoors();
		}
	}

	private void SetupDoors()
	{
		var currentRoomNode = RunManager.Instance?.CurrentRoom;

		bool hasLeftNeighbor = currentRoomNode != null && currentRoomNode.Neighbors.ContainsKey(new Vector2I(-1, 0));
		bool hasRightNeighbor = currentRoomNode != null && currentRoomNode.Neighbors.ContainsKey(new Vector2I(1, 0));

		if (_leftDoor != null)
		{
			if (IsLeftDoorClosed || !hasLeftNeighbor)
			{
				_leftDoor.Monitoring = false;
				_leftDoor.Visible = false;
			}
			else
			{
				_leftDoor.BodyEntered += OnLeftDoorEntered;
			}
		}

		if (_rightDoor != null)
		{
			if (IsRightDoorClosed || !hasRightNeighbor)
			{
				_rightDoor.Monitoring = false;
				_rightDoor.Visible = false;
			}
			else
			{
				_rightDoor.BodyEntered += OnRightDoorEntered;
			}
		}
	}

	private void OnLeftDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning && !_isBattleActive)
		{
			Vector2I targetDir = new Vector2I(-1, 0);
			if (CanMoveToDirection(targetDir))
			{
				_isTransitioning = true;
				RunManager.Instance?.MoveToRoom(targetDir, "SpawnRight");
			}
		}
	}

	private void OnRightDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning && !_isBattleActive)
		{
			Vector2I targetDir = new Vector2I(1, 0);
			if (CanMoveToDirection(targetDir))
			{
				_isTransitioning = true;
				RunManager.Instance?.MoveToRoom(targetDir, "SpawnLeft");
			}
		}
	}

	private bool CanMoveToDirection(Vector2I dir)
	{
		if (RunManager.Instance?.CurrentRoom == null) return false;
		Vector2I targetGridPos = RunManager.Instance.CurrentRoom.GridPos + dir;
		return RunManager.Instance.RoomGrid.ContainsKey(targetGridPos);
	}
}

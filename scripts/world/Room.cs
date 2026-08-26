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

	public override void _Ready()
	{
		GenerateProceduralPlatforms();
		SetupDoorsAndSpawns();

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
		// Самый нижний ярус теперь на высоте 920 (почти у земли), а верхний — 600
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
					// Уменьшили минимальную дистанцию, чтобы они могли стоять ближе друг к другу
					if (newPos.DistanceTo(existingPos) < 180f)
					{
						tooClose = true;
						break;
					}
				}

				if (tooClose) continue;
				spawnedPositions.Add(newPos);

				// Делаем платформы длиннее (от 250 до 400 пикселей), чтобы на них было проще приземляться
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

	private void SetupDoorsAndSpawns()
	{
		float rightWallX = LimitRight; 

		var rightDoor = GetNodeOrNull<Area2D>("RightDoor");
		if (rightDoor != null)
		{
			var pos = rightDoor.GlobalPosition;
			pos.X = rightWallX - 40; 
			rightDoor.GlobalPosition = pos;
		}

		var spawnRight = GetNodeOrNull<Marker2D>("SpawnRight");
		if (spawnRight != null)
		{
			var pos = spawnRight.GlobalPosition;
			pos.X = rightWallX - 60; 
			spawnRight.GlobalPosition = pos;
		}

		var leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
		if (leftDoor != null)
		{
			var pos = leftDoor.GlobalPosition;
			pos.X = 40;
			leftDoor.GlobalPosition = pos;
		}

		var spawnLeft = GetNodeOrNull<Marker2D>("SpawnLeft");
		if (spawnLeft != null)
		{
			var pos = spawnLeft.GlobalPosition;
			pos.X = 60;
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

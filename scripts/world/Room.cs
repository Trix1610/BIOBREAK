using Godot;
using System.Collections.Generic;

public partial class Room : Node2D
{
	[Export] public bool IsLeftDoorClosed { get; set; } = false;
	[Export] public bool IsRightDoorClosed { get; set; } = false;

	// Новые настройки границ камеры для каждой комнаты прямо в инспекторе
	[Export] public int LimitLeft { get; set; } = 0;
	[Export] public int LimitTop { get; set; } = 0;
	[Export] public int LimitRight { get; set; } = 1920;  // Можешь менять под ширину комнаты
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
		// 0. Автоматически выравниваем двери и точки спавна по границам комнаты
		SetupDoorsAndSpawns();

		// 1. Инициализация игрока (если его нет на сцене)
		Node2D player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		if (player == null && _playerScene != null)
		{
			player = _playerScene.Instantiate<Node2D>();
			AddChild(player);
		}

		// 2. Позиционирование на точку спавна
		string targetSpawnName = GameManager.Instance?.TargetSpawnPoint;
		if (!string.IsNullOrEmpty(targetSpawnName))
		{
			Marker2D spawnPoint = GetNodeOrNull<Marker2D>(targetSpawnName);
			if (spawnPoint != null && player != null)
			{
				player.GlobalPosition = spawnPoint.GlobalPosition;
			}
		}

		// 3. Теперь, когда игрок гарантированно существует и добавлен в дерево, настраиваем камеру
		var camera = player?.GetNodeOrNull<Camera2D>("Camera2D");
		if (camera != null)
		{
			// Сначала жестко задаем лимиты
			camera.LimitLeft = LimitLeft;
			camera.LimitTop = LimitTop;
			camera.LimitRight = LimitRight;
			camera.LimitBottom = LimitBottom;
			
			camera.LimitSmoothed = true;
			camera.PositionSmoothingEnabled = true;
			camera.DragHorizontalEnabled = false;
			camera.DragVerticalEnabled = false;

			// ГЛАВНОЕ ИСПРАВЛЕНИЕ: принудительно сбрасываем позицию камеры на игрока 
			// в первый же кадр, чтобы она не отставала и не теряла персонажа при спавне
			camera.ResetSmoothing();
			camera.GlobalPosition = player.GlobalPosition;
		}

		_leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
		_rightDoor = GetNodeOrNull<Area2D>("RightDoor");

		SetupDoors();
		CheckRoomClearance();
	}

	private void SetupDoorsAndSpawns()
	{
		// Получаем реальную правую границу комнаты из лимитов камеры или текстуры фона
		float rightWallX = LimitRight; 

		// Правая дверь (ставим прямо у самой стены, отступив всего 40 пикселей внутрь)
		var rightDoor = GetNodeOrNull<Area2D>("RightDoor");
		if (rightDoor != null)
		{
			var pos = rightDoor.GlobalPosition;
			pos.X = rightWallX - 40; 
			rightDoor.GlobalPosition = pos;
		}

		// Правый спавн (ставим вплотную к стене, отступив 60 пикселей, чтобы игрок не застревал в коллизии)
		var spawnRight = GetNodeOrNull<Marker2D>("SpawnRight");
		if (spawnRight != null)
		{
			var pos = spawnRight.GlobalPosition;
			pos.X = rightWallX - 60; 
			spawnRight.GlobalPosition = pos;
		}

		// То же самое для левой стороны (на всякий случай)
		var leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
		var spawnLeft = GetNodeOrNull<Marker2D>("SpawnLeft");
		if (leftDoor != null)
		{
			var pos = leftDoor.GlobalPosition;
			pos.X = 40;
			leftDoor.GlobalPosition = pos;
		}
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
		if (EnemyScene == null)
		{
			GD.PrintErr("[Room] Ошибка: EnemyScene не назначена в Room.cs!");
			return;
		}

		_activeEnemies.Clear();

		Vector2 screenSize = GetViewportRect().Size;
		var random = new RandomNumberGenerator();
		random.Randomize();

		int enemiesToSpawn = 3;

		for (int i = 0; i < enemiesToSpawn; i++)
		{
			Node2D enemyInstance = EnemyScene.Instantiate<Node2D>();
			AddChild(enemyInstance);

			// Подписываемся на выход из дерева сцены
			enemyInstance.TreeExited += OnEnemyDefeated;
			_activeEnemies.Add(enemyInstance);

			float randomX = random.RandfRange(150.0f, screenSize.X - 150.0f);
			float randomY = random.RandfRange(150.0f, screenSize.Y - 150.0f);

			enemyInstance.GlobalPosition = new Vector2(randomX, randomY);
		}

		GD.Print($"[Room] Заспавнено врагов: {_activeEnemies.Count}");
	}

	private void LockDoors()
	{
		_isBattleActive = true;

		if (_leftDoor != null) _leftDoor.Monitoring = false;
		if (_rightDoor != null) _rightDoor.Monitoring = false;

		GD.Print($"[Room] Бой начался! Двери заблокированы.");
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

		GD.Print("[Room] Все враги повержены! Двери открыты.");
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
		// Важная задержка: удаляем врагов на следующем кадре физики, когда узел точно покинул дерево
		CallDeferred(nameof(CheckEnemiesCount));
	}

	private void CheckEnemiesCount()
	{
		_activeEnemies.RemoveAll(e => !GodotObject.IsInstanceValid(e) || !e.IsInsideTree());

		GD.Print($"[Room] Враг уничтожен! Осталось врагов: {_activeEnemies.Count}");

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
			else
			{
				GD.Print("[Room] Соседней комнаты слева нет на сетке, переход отменен.");
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
			else
			{
				GD.Print("[Room] Соседней комнаты справа нет на сетке, переход отменен.");
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

using Godot;
using System.Collections.Generic;

public partial class RunManager : Node
{
	public static RunManager Instance { get; private set; }

	// Флаг активности текущего забега
	public bool IsRunActive { get; private set; } = false;

	public RoomNode CurrentRoom { get; set; }
	public Dictionary<Vector2I, RoomNode> RoomGrid { get; private set; } = new();

	// Ссылка на черный экран для фейда
	private ColorRect _fadeRect;

	// Ссылка на экран смерти и его префаб (укажи правильный путь к своей сцене GameOverScreen)
	private PackedScene _gameOverScene = GD.Load<PackedScene>("res://scenes/ui/GameOverScreen.tscn");
	private Control _gameOverInstance;

	// Префайбы комнат
	private PackedScene _mainScene = GD.Load<PackedScene>("res://scenes/world/Main.tscn");
	private PackedScene _lvl2Scene = GD.Load<PackedScene>("res://scenes/world/Level2.tscn");
	private PackedScene _lvl3Scene = GD.Load<PackedScene>("res://scenes/world/Level3.tscn");
	private PackedScene _lvl4Scene = GD.Load<PackedScene>("res://scenes/world/Level4.tscn");
	private PackedScene _lvl5Scene = GD.Load<PackedScene>("res://scenes/world/Level5.tscn");

	public override void _Ready()
	{
		Instance = this;

		// Автоматически создаем слой затемнения поверх всех окон при запуске
		SetupFadeLayer();

		// Автоматически запускаем раунд при старте, если он еще не активен
		if (!IsRunActive)
		{
			StartNewRun();
		}
	}

	private void SetupFadeLayer()
	{
		var canvasLayer = new CanvasLayer();
		canvasLayer.Layer = 100; // Ставим поверх всего интерфейса и комнат
		AddChild(canvasLayer);

		_fadeRect = new ColorRect();
		_fadeRect.Color = Colors.Black;
		_fadeRect.Modulate = new Color(1, 1, 1, 0); // Изначально полностью прозрачный
		
		// Растягиваем на весь экран
		_fadeRect.AnchorRight = 1.0f;
		_fadeRect.AnchorBottom = 1.0f;
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore; // Чтобы не блокировал клики мыши
		
		canvasLayer.AddChild(_fadeRect);
	}

	// МЕТОД ЗАПУСКА НОВОГО ЗАБЕГА
	public void StartNewRun(int runSeed = 0)
	{
		// Очищаем экран смерти, если он остался с прошлого раза
		ClearGameOverScreen();

		IsRunActive = true;
		GenerateFixedLinearFloor();

		if (CurrentRoom != null && CurrentRoom.RoomScene != null)
		{
			if (GameManager.Instance != null)
			{
				GameManager.Instance.TargetSpawnPoint = "SpawnCenter";
			}

			CallDeferred(MethodName.ChangeSceneDeferred, CurrentRoom.RoomScene.ResourcePath);
		}
	}

	private void GenerateFixedLinearFloor()
	{
		RoomGrid.Clear();

		// 1. Создаем узлы комнат
		var mainRoom = new RoomNode(new Vector2I(0, 0), _mainScene);
		var lvl2Room = new RoomNode(new Vector2I(-1, 0), _lvl2Scene);
		var lvl3Room = new RoomNode(new Vector2I(-2, 0), _lvl3Scene);
		var lvl4Room = new RoomNode(new Vector2I(1, 0), _lvl4Scene);
		var lvl5Room = new RoomNode(new Vector2I(2, 0), _lvl5Scene);

		// 2. Регистрируем в сетке
		RoomGrid[mainRoom.GridPos] = mainRoom;
		RoomGrid[lvl2Room.GridPos] = lvl2Room;
		RoomGrid[lvl3Room.GridPos] = lvl3Room;
		RoomGrid[lvl4Room.GridPos] = lvl4Room;
		RoomGrid[lvl5Room.GridPos] = lvl5Room;

		// 3. Соединяем соседей
		ConnectRooms(mainRoom, lvl2Room, new Vector2I(-1, 0));
		ConnectRooms(lvl2Room, lvl3Room, new Vector2I(-1, 0));
		ConnectRooms(mainRoom, lvl4Room, new Vector2I(1, 0));
		ConnectRooms(lvl4Room, lvl5Room, new Vector2I(1, 0));

		// Стартовая комната — Main (0, 0)
		CurrentRoom = mainRoom;
		CurrentRoom.IsVisited = true;
		CurrentRoom.IsCleared = true;
	}

	private void ConnectRooms(RoomNode roomA, RoomNode roomB, Vector2I directionFromAToB)
	{
		roomA.Neighbors[directionFromAToB] = roomB;
		roomB.Neighbors[-directionFromAToB] = roomA;
	}

	public async void MoveToRoom(Vector2I direction, string targetSpawnPoint)
	{
		GD.Print($"[RunManager] Попытка пойти из комнаты {CurrentRoom.GridPos} в направлении {direction}");
		
		if (CurrentRoom != null && CurrentRoom.Neighbors.ContainsKey(direction))
		{
			RoomNode nextRoom = CurrentRoom.Neighbors[direction];
			
			GD.Print($"[RunManager] Успех! Следующая комната: {nextRoom.GridPos}, сцена: {nextRoom.RoomScene.ResourcePath}");
		
			CurrentRoom = nextRoom;
			CurrentRoom.IsVisited = true;

			if (GameManager.Instance != null)
			{
				GameManager.Instance.TargetSpawnPoint = targetSpawnPoint;
			}

			if (_fadeRect != null)
			{
				// 1. Плавно затемняем экран
				Tween tweenIn = CreateTween();
				tweenIn.TweenProperty(_fadeRect, "modulate:a", 1.0f, 0.2f);
				await ToSignal(tweenIn, "finished");

				// 2. Меняем сцену в темноте
				CallDeferred(MethodName.ChangeSceneDeferred, nextRoom.RoomScene.ResourcePath);

				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");

				// 3. Плавно возвращаем видимость обратно
				Tween tweenOut = CreateTween();
				tweenOut.TweenProperty(_fadeRect, "modulate:a", 0.0f, 0.2f);
			}
			else
			{
				CallDeferred(MethodName.ChangeSceneDeferred, nextRoom.RoomScene.ResourcePath);
			}
		}
		else
		{
			GD.Print($"[RunManager] Дверь есть, но комната в направлении {direction} отсутствует!");
		}
	}

	// --- МЕТОДЫ ДЛЯ РАБОТЫ С GAME OVER ---

	public void ShowGameOver()
	{
		// Если экран смерти еще не создан, инстанциируем его
		if (_gameOverInstance == null && _gameOverScene != null)
		{
			_gameOverInstance = _gameOverScene.Instantiate<Control>();
			
			// Вешаем его на тот же CanvasLayer, где живет _fadeRect (слой 100 — поверх всего)
			if (_fadeRect != null && _fadeRect.GetParent() is CanvasLayer canvasLayer)
			{
				canvasLayer.AddChild(_gameOverInstance);
			}
		}

		if (_gameOverInstance != null)
		{
			_gameOverInstance.Visible = true;
			// Убеждаемся, что экран и его кнопки работают даже во время паузы игры
			_gameOverInstance.ProcessMode = Node.ProcessModeEnum.Always;
		}

		// Ставим игру на паузу
		GetTree().Paused = true;
	}

	private void ClearGameOverScreen()
	{
		if (_gameOverInstance != null)
		{
			_gameOverInstance.Visible = false;
			_gameOverInstance.QueueFree();
			_gameOverInstance = null;
		}
	}

	// Универсальный метод рестарта (вызывай его из кнопки рестарта на экране смерти)
	public void RestartGame()
	{
		GetTree().Paused = false; // Обязательно снимаем игру с паузы
		StartNewRun();            // Запускаем забег с чистого листа
	}

	private void ChangeSceneDeferred(string scenePath)
	{
		GetTree().ChangeSceneToFile(scenePath);
	}
}

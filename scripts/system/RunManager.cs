using Godot;
using System;
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
		IsRunActive = true;
		GenerateFixedLinearFloor();

		// ГЛАВНОЕ ИСПРАВЛЕНИЕ: принудительно загружаем сцену Main при старте забега,
		// чтобы игра физически открывала правильную комнату с нужными точками спавна!
		if (CurrentRoom != null && CurrentRoom.RoomScene != null)
		{
			// Указываем точку спавна по умолчанию для старта игры
			if (GameManager.Instance != null)
			{
				GameManager.Instance.TargetSpawnPoint = "SpawnCenter"; // Или "SpawnLeft"
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
		if (CurrentRoom != null && CurrentRoom.Neighbors.ContainsKey(direction))
		{
			RoomNode nextRoom = CurrentRoom.Neighbors[direction];
			CurrentRoom = nextRoom;
			CurrentRoom.IsVisited = true;

			if (GameManager.Instance != null)
			{
				GameManager.Instance.TargetSpawnPoint = targetSpawnPoint;
			}

			// Если экран затемнения существует, делаем плавный переход
			if (_fadeRect != null)
			{
				// 1. Плавно затемняем экран (за 0.2 секунды)
				Tween tweenIn = CreateTween();
				tweenIn.TweenProperty(_fadeRect, "modulate:a", 1.0f, 0.2f);
				await ToSignal(tweenIn, "finished");

				// 2. Меняем сцену в темноте
				CallDeferred(MethodName.ChangeSceneDeferred, nextRoom.RoomScene.ResourcePath);

				// Небольшая пауза для стабилизации прогрузки новой сцены
				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");

				// 3. Плавно возвращаем видимость обратно
				Tween tweenOut = CreateTween();
				tweenOut.TweenProperty(_fadeRect, "modulate:a", 0.0f, 0.2f);
			}
			else
			{
				// Резервный вариант без анимации, если вдруг что-то пойдет не так
				CallDeferred(MethodName.ChangeSceneDeferred, nextRoom.RoomScene.ResourcePath);
			}
		}
		else
		{
			GD.Print($"[RunManager] Door exists, but no room exists in direction: {direction}");
		}
	}

	private void ChangeSceneDeferred(string scenePath)
	{
		GetTree().ChangeSceneToFile(scenePath);
	}
}

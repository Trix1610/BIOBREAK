using Godot;
using System.Collections.Generic;

public partial class RunManager : Node
{
	public static RunManager Instance { get; private set; }

	// Флаг активности текущего забега
	public bool IsRunActive { get; private set; } = false;

	public RoomNode CurrentRoom { get; set; }
	public Dictionary<Vector2I, RoomNode> RoomGrid { get; private set; } = new();

	// Префайбы комнат
	private PackedScene _mainScene = GD.Load<PackedScene>("res://scenes/world/Main.tscn");
	private PackedScene _lvl2Scene = GD.Load<PackedScene>("res://scenes/world/level2.tscn");
	private PackedScene _lvl3Scene = GD.Load<PackedScene>("res://scenes/world/level3.tscn");
	private PackedScene _lvl4Scene = GD.Load<PackedScene>("res://scenes/world/level4.tscn");
	private PackedScene _lvl5Scene = GD.Load<PackedScene>("res://scenes/world/level5.tscn");

	public override void _Ready()
	{
		Instance = this;
		
		// Автоматически запускаем раунд при старте, если он еще не активен
		if (!IsRunActive)
		{
			StartNewRun();
		}
	}

	// МЕТОД ЗАПУСКА НОВОГО ЗАБЕГА
	public void StartNewRun(int runSeed = 0)
	{
		IsRunActive = true;
		GenerateFixedLinearFloor();
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
	}

	private void ConnectRooms(RoomNode roomA, RoomNode roomB, Vector2I directionFromAToB)
	{
		roomA.Neighbors[directionFromAToB] = roomB;
		roomB.Neighbors[-directionFromAToB] = roomA;
	}

	public void MoveToRoom(Vector2I direction, string targetSpawnPoint)
	{
		if (CurrentRoom != null && CurrentRoom.Neighbors.ContainsKey(direction))
		{
			RoomNode nextRoom = CurrentRoom.Neighbors[direction];
			CurrentRoom = nextRoom;
			CurrentRoom.IsVisited = true;

			GameManager.Instance.TargetSpawnPoint = targetSpawnPoint;

			// ВМЕСТО ПРЯМОГО ВЫЗОВА: GetTree().ChangeSceneToFile(...)
			// Используем CallDeferred для безопасной смены сцены после физического шага:
			CallDeferred(MethodName.ChangeSceneDeferred, nextRoom.RoomScene.ResourcePath);
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

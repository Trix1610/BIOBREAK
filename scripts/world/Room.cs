using Godot;

public partial class Room : Node2D
{
	[Export] public int LimitLeft { get; set; } = 0;
	[Export] public int LimitTop { get; set; } = 0;
	[Export] public int LimitRight { get; set; } = 1920;
	[Export] public int LimitBottom { get; set; } = 1080;

	[Export] public PackedScene EnemyScene { get; set; } = GD.Load<PackedScene>("res://scenes/enemies/Enemy.tscn");

	private PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");

	private PlatformGenerator _platformGenerator;
	private DoorController _doorController;
	private RoomEnemySpawner _enemySpawner;
	private RoomKillZone _killZone;

	public async override void _Ready()
	{
		try
		{
			// Создаем компоненты
			InitializeComponents();

			// Генерируем платформы
			Vector2I roomGridPos = RunManager.Instance?.CurrentRoom?.GridPos ?? new Vector2I(0, 0);
			_platformGenerator.GeneratePlatforms(this, roomGridPos);

			// Настраиваем двери и спавны
			_doorController.SetupDoorsAndSpawns(this);

			// Настраиваем KillZone
			_killZone.SetupKillZone(this, _enemySpawner);

			// Настраиваем игрока
			Node2D player = SetupPlayer();

			// Настраиваем камеру
			SetupCamera(player);

			// Инициализируем двери
			_doorController.InitializeDoors(this);

			// Проверяем зачистку комнаты
			CheckRoomClearance();

			// ЗАЩИТА ОТ ДВОЙНОГО СРАБАТЫВАНИЯ:
			// Временно выключаем двери при входе в комнату
			_doorController.TemporarilyDisableDoors();

			// Ждем 0.3 секунды, давая игроку время сойти с точки спавна/двери
			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

			// Включаем обратно только те двери, которые должны работать
			_doorController.EnableDoorsAfterDelay();

		}
		catch (System.Exception ex)
		{
			GD.PrintErr($"[Room] Ошибка в _Ready: {ex.Message}");
		}
	}

	private void InitializeComponents()
	{
		_platformGenerator = new PlatformGenerator();
		_platformGenerator.LimitRight = LimitRight;
		_platformGenerator.Name = "PlatformGenerator";
		AddChild(_platformGenerator);

		_doorController = new DoorController();
		_doorController.LimitRight = LimitRight;
		_doorController.Name = "DoorController";
		AddChild(_doorController);

		_enemySpawner = new RoomEnemySpawner();
		_enemySpawner.EnemyScene = EnemyScene;
		_enemySpawner.Name = "RoomEnemySpawner";
		AddChild(_enemySpawner);

		_killZone = new RoomKillZone();
		_killZone.LimitRight = LimitRight;
		_killZone.LimitBottom = LimitBottom;
		_killZone.Name = "RoomKillZone";
		AddChild(_killZone);

		// Подписываемся на события
		_enemySpawner.OnAllEnemiesDefeated += OnAllEnemiesDefeated;
		_doorController.OnRoomTransitionRequested += OnRoomTransitionRequested;
	}

	private Node2D SetupPlayer()
	{
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

		return player;
	}

	private void SetupCamera(Node2D player)
	{
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
	}

	private void CheckRoomClearance()
	{
		var currentRoomNode = RunManager.Instance?.CurrentRoom;

		if (currentRoomNode != null && !currentRoomNode.IsCleared)
		{
			_enemySpawner.SpawnEnemies(this);

			if (_enemySpawner.GetActiveEnemyCount() > 0)
			{
				_doorController.LockDoors();
			}
			else
			{
				currentRoomNode.IsCleared = true;
			}
		}
	}

	private void OnAllEnemiesDefeated()
	{
		GD.Print("[Room] Все враги повержены! Открываем двери.");
		_doorController.UnlockDoors();
	}

	private void OnRoomTransitionRequested()
	{
		GD.Print("[Room] Переход в другую комнату инициирован.");
	}
}

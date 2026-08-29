using Godot;
using System.Collections.Generic;

public partial class RunManager : Node
{
	public static RunManager Instance { get; private set; }

	// Флаг активности текущего забега
	public bool IsRunActive { get; private set; } = false;

	public RoomNode CurrentRoom { get; set; }
	public Dictionary<Vector2I, RoomNode> RoomGrid { get; private set; } = new();

	// Сохранение текущего оружия игрока между комнатами
	public WeaponData CurrentWeaponData { get; set; }

	// Ссылка на черный экран для фейда
	private ColorRect _fadeRect;

	// Ссылка на экран смерти и его префаб
	private PackedScene _gameOverScene = GD.Load<PackedScene>("res://scenes/ui/GameOverScreen.tscn");
	private Control _gameOverInstance;

	// Ссылка на меню паузы и его префаб
	private PackedScene _pauseMenuScene = GD.Load<PackedScene>("res://scenes/ui/PauseMenu.tscn");
	private Control _pauseMenuInstance;
	private bool _isPaused = false;

	// Префайбы комнат
	private PackedScene _mainScene = GD.Load<PackedScene>("res://scenes/world/Main.tscn");
	private PackedScene _lvl2Scene = GD.Load<PackedScene>("res://scenes/world/Level2.tscn");
	private PackedScene _lvl3Scene = GD.Load<PackedScene>("res://scenes/world/Level3.tscn");
	private PackedScene _lvl4Scene = GD.Load<PackedScene>("res://scenes/world/Level4.tscn");
	private PackedScene _lvl5Scene = GD.Load<PackedScene>("res://scenes/world/Level5.tscn");

	// Путь к файлу настроек
	private const string SettingsFilePath = "user://settings.cfg";

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;

		// Заставляем RunManager работать даже во время паузы для обработки Esc
		ProcessMode = ProcessModeEnum.Always;

		// Применяем сохраненные настройки экрана (полноэкранный режим / разрешение) при запуске
		ApplySavedSettings();

		// Автоматически создаем слой затемнения поверх всех окон при запуске
		SetupFadeLayer();
	}

	// --- МЕТОДЫ ДЛЯ РАБОТЫ С НАСТРОЙКАМИ ЭКРАНА ---

	private void ApplySavedSettings()
	{
		ConfigFile config = new ConfigFile();
		Error err = config.Load(SettingsFilePath);
		
		if (err == Error.Ok)
		{
			// Читаем сохраненный Fullscreen (по умолчанию false - оконный режим)
			bool isFullscreen = (bool.TryParse(config.GetValue("Video", "Fullscreen", false).ToString(), out var fs)) ? fs : false;

			if (isFullscreen)
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				GD.Print("[RunManager] Применен сохраненный режим: Fullscreen");
			}
			else
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				GD.Print("[RunManager] Применен сохраненный режим: Windowed");
			}
		}
		else
		{
			// Если файла настроек еще нет, запускаем в обычном оконном режиме
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
	}

	// Метод для сохранения настроек (можешь вызывать из SettingsMenu)
	public static void SaveVideoSettings(bool isFullscreen)
	{
		ConfigFile config = new ConfigFile();
		config.Load(SettingsFilePath); // Загружаем существующие, чтобы не затереть звук, если он там есть
		
		config.SetValue("Video", "Fullscreen", isFullscreen);
		config.Save(SettingsFilePath);
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

	public override void _UnhandledInput(InputEvent @event)
	{
		// Проверяем Esc только если забег активен
		if (@event.IsActionPressed("ui_cancel") && IsRunActive)
		{
			// Дополнительная защита: если мы случайно оказались в сцене главного меню, не открываем паузу
			string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
			if (currentScenePath.Contains("MainMenu"))
			{
				return;
			}

			TogglePause();
		}
	}
	
	// Вызывается при выходе в главное меню или завершении сессии
	public void EndRun()
	{
		IsRunActive = false;
		_isPaused = false;
		GetTree().Paused = false;
		ClearPauseMenu();
		ClearGameOverScreen();

		// Скрываем UI при выходе в меню
		if (HealthUI.Instance != null)
		{
			HealthUI.Instance.Visible = false;
		}

		if (Minimap.Instance != null)
		{
			Minimap.Instance.Visible = false;
		}
	}

	public void TogglePause()
	{
		// Инвертируем текущий статус паузы
		_isPaused = !_isPaused;

		if (_isPaused)
		{
			// Если ставим на паузу — создаем меню, если его еще не было
			if (_pauseMenuInstance == null && _pauseMenuScene != null)
			{
				_pauseMenuInstance = _pauseMenuScene.Instantiate<Control>();
				
				if (_fadeRect != null && _fadeRect.GetParent() is CanvasLayer canvasLayer)
				{
					canvasLayer.AddChild(_pauseMenuInstance);
				}
			}

			if (_pauseMenuInstance != null)
			{
				_pauseMenuInstance.Visible = true;
			}

			GetTree().Paused = true;
		}
		else
		{
			// Если снимаем с паузы — просто скрываем меню паузы
			if (_pauseMenuInstance != null)
			{
				_pauseMenuInstance.Visible = false;
			}

			GetTree().Paused = false;
		}
	}

	// МЕТОД ЗАПУСКА НОВОГО ЗАБЕГА
	public async void StartNewRun(int runSeed = 0)
	{
		// На всякий случай снимаем паузу и очищаем старые окна при новом забеге
		GetTree().Paused = false;
		_isPaused = false;
		ClearPauseMenu();
		ClearGameOverScreen();

		// Сбрасываем оружие на стартовый пистолет при начале нового забега
		CurrentWeaponData = GD.Load<WeaponData>("res://resources/weapons/projectile/pistol.tres");

		// Возвращаем видимость UI при начале нового забега
		if (HealthUI.Instance != null)
		{
			HealthUI.Instance.Visible = true;
		}

		if (Minimap.Instance != null)
		{
			Minimap.Instance.Visible = true;
		}

		// Мгновенно закрываем экран черным полотном, скрывая кадры инициализации
		if (_fadeRect != null)
		{
			_fadeRect.Modulate = new Color(1, 1, 1, 1);
		}

		IsRunActive = true;
		GenerateFixedLinearFloor();

		if (CurrentRoom != null && CurrentRoom.RoomScene != null)
		{
			if (GameManager.Instance != null)
			{
				GameManager.Instance.TargetSpawnPoint = "SpawnCenter";
			}

			// Меняем сцену
			GetTree().ChangeSceneToFile(CurrentRoom.RoomScene.ResourcePath);

			// Ждем полной инициализации узлов и камеры в новой сцене
			await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			// Плавно проявляем готовую сцену из темноты
			if (_fadeRect != null)
			{
				Tween tweenOut = CreateTween();
				tweenOut.TweenProperty(_fadeRect, "modulate:a", 0.0f, 0.3f);
			}
		}
	}

	// Метод для обновления и сохранения текущего оружия игрока
	public void UpdatePlayerWeapon(WeaponData newWeaponData)
	{
		CurrentWeaponData = newWeaponData;

		// Если игрок уже на сцене, сразу выдаем ему новое оружие
		var player = GetTree()?.GetFirstNodeInGroup("Player") as Player;
		if (player != null)
		{
			player.EquipWeapon(newWeaponData);
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
				// 1. Плавно затемняем экран перед переходом
				Tween tweenIn = CreateTween();
				tweenIn.TweenProperty(_fadeRect, "modulate:a", 1.0f, 0.2f);
				await ToSignal(tweenIn, "finished");

				// 2. Меняем сцену в темноте
				GetTree().ChangeSceneToFile(nextRoom.RoomScene.ResourcePath);

				// Ждем завершения смены сцены
				await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

				// 3. Плавно возвращаем видимость обратно
				Tween tweenOut = CreateTween();
				tweenOut.TweenProperty(_fadeRect, "modulate:a", 0.0f, 0.2f);
			}
			else
			{
				GetTree().ChangeSceneToFile(nextRoom.RoomScene.ResourcePath);
			}
		}
		else
		{
			GD.Print($"[RunManager] Дверь есть, но комната в направлении {direction} отсутствует!");
		}
	}

	// --- МЕТОДЫ ДЛЯ РАБОТЫ С GAME OVER И ПАУЗОЙ ---

	public void ShowGameOver()
	{
		// Выключаем меню паузы, если оно горело
		_isPaused = false;
		ClearPauseMenu();

		if (_gameOverInstance == null && _gameOverScene != null)
		{
			_gameOverInstance = _gameOverScene.Instantiate<Control>();
			
			if (_fadeRect != null && _fadeRect.GetParent() is CanvasLayer canvasLayer)
			{
				canvasLayer.AddChild(_gameOverInstance);
			}
		}

		if (_gameOverInstance != null)
		{
			_gameOverInstance.Visible = true;
			_gameOverInstance.ProcessMode = Node.ProcessModeEnum.Always;
		}

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

	private void ClearPauseMenu()
	{
		if (_pauseMenuInstance != null)
		{
			_pauseMenuInstance.Visible = false;
			_pauseMenuInstance.QueueFree();
			_pauseMenuInstance = null;
		}
	}

	public void RestartGame()
	{
		StartNewRun();         
	}
}

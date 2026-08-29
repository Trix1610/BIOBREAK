using Godot;

public partial class GameOverScreen : Control
{
	private Button _restartButton;
	private Button _quitButton;

	// Массив кнопок для управления закольцованной навигацией
	private Button[] _menuButtons;
	private int _currentIndex = 0;

	[Export(PropertyHint.File, "*.tscn")]
	private string MainMenuScenePath { get; set; } = "res://scenes/ui/MainMenu.tscn";

	public override void _Ready()
	{
		// Меню смерти появляется при паузе игры, поэтому оно должно работать в режиме Always
		ProcessMode = ProcessModeEnum.Always;

		// Находим кнопки (пути можно подкорректировать, если они лежат иначе)
		_restartButton = GetNodeOrNull<Button>("VBoxContainer/RestartButton");
		_quitButton = GetNodeOrNull<Button>("VBoxContainer/QuitButton");

		if (_restartButton != null)
		{
			_restartButton.ProcessMode = ProcessModeEnum.Always;
			_restartButton.Pressed += OnRestartPressed;
		}
		else
		{
			GD.PrintErr("GameOverScreen: Не удалось найти кнопку рестарта!");
		}

		if (_quitButton != null)
		{
			_quitButton.ProcessMode = ProcessModeEnum.Always;
			_quitButton.Pressed += OnQuitPressed;
		}
		else
		{
			GD.PrintErr("GameOverScreen: Не удалось найти кнопку выхода в меню!");
		}

		// Собираем кнопки в массив для навигации
		_menuButtons = new Button[] { _restartButton, _quitButton };

		// Сразу даем фокус на кнопку рестарта, чтобы работало управление с клавиатуры / геймпада
		_currentIndex = 0;
		_restartButton?.GrabFocus();
	}

	public override void _Input(InputEvent @event)
	{
		if (!Visible) return;
		if (_menuButtons == null || _menuButtons.Length == 0) return;

		// Обработка нажатия вверх
		if (@event.IsActionPressed("ui_up"))
		{
			_currentIndex--;
			if (_currentIndex < 0)
			{
				// Если дошли до верха и нажали еще раз — переходим на самую нижнюю кнопку
				_currentIndex = _menuButtons.Length - 1;
			}

			UpdateFocus();
			GetViewport().SetInputAsHandled();
		}
		// Обработка нажатия вниз
		else if (@event.IsActionPressed("ui_down"))
		{
			_currentIndex++;
			if (_currentIndex >= _menuButtons.Length)
			{
				// Если дошли до низа и нажали еще раз — переходим на самую верхнюю кнопку
				_currentIndex = 0;
			}

			UpdateFocus();
			GetViewport().SetInputAsHandled();
		}
	}

	private void UpdateFocus()
	{
		if (_menuButtons[_currentIndex] != null)
		{
			_menuButtons[_currentIndex].GrabFocus();
		}
	}

	private void OnRestartPressed()
	{
		GD.Print("Нажата кнопка рестарта!");

		if (RunManager.Instance != null)
		{
			RunManager.Instance.RestartGame();
		}
		else
		{
			GetTree().Paused = false;
			GetTree().ReloadCurrentScene();
		}
	}

	private void OnQuitPressed()
	{
		GD.Print("Нажата кнопка выхода в главное меню из экрана смерти!");

		// Обязательно снимаем игру с паузы перед переключением сцены
		GetTree().Paused = false;

		// Сбрасываем состояние забега в RunManager, если такой метод есть
		if (RunManager.Instance != null)
		{
			RunManager.Instance.EndRun();
		}

		// Переходим в главное меню
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
}

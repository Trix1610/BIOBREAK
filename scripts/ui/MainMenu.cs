using Godot;

public partial class MainMenu : Control
{
	private Button _startButton;
	private Button _settingsButton;
	private Button _quitButton;

	// Массив кнопок для управления навигацией
	private Button[] _menuButtons;
	private int _currentIndex = 0;

	[Export(PropertyHint.File, "*.tscn")]
	private string GameScenePath { get; set; } = "res://scenes/main.tscn";

	// Ссылка на панель настроек
	[Export] private NodePath SettingsPanelPath;
	private Control _settingsPanel;

	public override void _Ready()
	{
		GD.Print("[MainMenu Debug] Метод _Ready() запущен! Главное меню успешно инициализировано.");

		// Проверяем, на месте ли пути к кнопкам
		_startButton = GetNodeOrNull<Button>("VBoxContainer/StartButton");
		_settingsButton = GetNodeOrNull<Button>("VBoxContainer/SettingsButton");
		_quitButton = GetNodeOrNull<Button>("VBoxContainer/QuitButton");

		_settingsPanel = GetNodeOrNull<Control>(SettingsPanelPath);

		if (_startButton != null)
		{
			GD.Print("[MainMenu Debug] Кнопка 'StartButton' успешно найдена.");
			_startButton.Pressed += OnStartPressed;
		}
		else
		{
			GD.PrintErr("[MainMenu Error] НЕ НАЙДЕНА кнопка по пути 'VBoxContainer/StartButton'!");
		}

		if (_settingsButton != null)
		{
			GD.Print("[MainMenu Debug] Кнопка 'SettingsButton' успешно найдена.");
			_settingsButton.Pressed += OnSettingsPressed;
		}
		else
		{
			GD.PrintErr("[MainMenu Error] НЕ НАЙДЕНА кнопка по пути 'VBoxContainer/SettingsButton'!");
		}

		if (_quitButton != null)
		{
			GD.Print("[MainMenu Debug] Кнопка 'QuitButton' успешно найдена.");
			_quitButton.Pressed += OnQuitPressed;
		}
		else
		{
			GD.PrintErr("[MainMenu Error] НЕ НАЙДЕНА кнопка по пути 'VBoxContainer/QuitButton'!");
		}

		// Собираем кнопки в массив для удобной навигации
		_menuButtons = new Button[] { _startButton, _settingsButton, _quitButton };

		// Автоматически даем фокус клавиатуры на кнопку Старт при открытии меню
		if (_menuButtons.Length > 0 && _menuButtons[0] != null)
		{
			_currentIndex = 0;
			_menuButtons[_currentIndex].GrabFocus();
		}
	}

	// Отслеживаем возвращение из настроек, чтобы кнопка «Настройки» не теряла фокус
	public override void _Process(double delta)
	{
		if (Visible && (_settingsPanel == null || !_settingsPanel.Visible))
		{
			var focusedNode = GetViewport().GuiGetFocusOwner();
			if (focusedNode == null || !IsAncestorOf(focusedNode))
			{
				UpdateFocus();
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Если открыты настройки, главное меню не должно перехватывать стрелочки
		if (_settingsPanel != null && _settingsPanel.Visible) return;
		if (!Visible) return;
		if (_menuButtons == null || _menuButtons.Length == 0) return;

		// Обработка нажатия вверх
		if (@event.IsActionPressed("ui_up"))
		{
			_currentIndex--;
			if (_currentIndex < 0)
			{
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
				_currentIndex = 0;
			}

			UpdateFocus();
			GetViewport().SetInputAsHandled();
		}
	}

	private void UpdateFocus()
	{
		if (_currentIndex >= 0 && _currentIndex < _menuButtons.Length && _menuButtons[_currentIndex] != null)
		{
			_menuButtons[_currentIndex].GrabFocus();
		}
	}

	private void OnStartPressed()
	{
		GD.Print("[MainMenu Action] Нажата кнопка Старт. Запуск нового забега...");
		
		if (RunManager.Instance != null)
		{
			RunManager.Instance.StartNewRun();
		}
		else
		{
			if (!string.IsNullOrEmpty(GameScenePath))
			{
				GetTree().ChangeSceneToFile(GameScenePath);
			}
		}
	}

	private void OnSettingsPressed()
	{
		GD.Print("[MainMenu Action] Нажата кнопка Настройки.");
		if (_settingsPanel != null)
		{
			// Запоминаем индекс кнопки настроек (в массиве она под индексом 1: Start, Settings, Quit)
			for (int i = 0; i < _menuButtons.Length; i++)
			{
				if (_menuButtons[i] == _settingsButton)
				{
					_currentIndex = i;
					break;
				}
			}

			_settingsPanel.Visible = true;
			
			// Ищем кнопку BackButton с учетом возможного CenterContainer в панели настроек
			var backButton = _settingsPanel.FindChild("BackButton", true, false) as Button;
			backButton?.GrabFocus();
		}
	}

	private void OnQuitPressed()
	{
		GD.Print("[MainMenu Action] Нажата кнопка Выход.");
		GetTree().Quit();
	}
}

using Godot;

public partial class PauseMenu : Control
{
	private Button _resumeButton;
	private Button _restartButton;
	private Button _settingsButton;
	private Button _quitButton;

	private Button[] _menuButtons;
	private int _currentIndex = 0;

	[Export(PropertyHint.File, "*.tscn")]
	private string MainMenuScenePath { get; set; } = "res://scenes/ui/MainMenu.tscn";

	[Export] private NodePath SettingsPanelPath;
	private Control _settingsPanel;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_resumeButton = GetNodeOrNull<Button>("VBoxContainer/ResumeButton");
		_restartButton = GetNodeOrNull<Button>("VBoxContainer/RestartButton");
		_settingsButton = GetNodeOrNull<Button>("VBoxContainer/SettingsButton");
		_quitButton = GetNodeOrNull<Button>("VBoxContainer/QuitButton");

		_settingsPanel = GetNodeOrNull<Control>(SettingsPanelPath);

		if (_resumeButton != null)
		{
			_resumeButton.ProcessMode = ProcessModeEnum.Always;
			_resumeButton.Pressed += OnResumePressed;
		}
		else { GD.PrintErr("[PauseMenu Error] ResumeButton не найдена по пути CenterContainer/VBoxContainer/ResumeButton!"); }

		if (_restartButton != null)
		{
			_restartButton.ProcessMode = ProcessModeEnum.Always;
			_restartButton.Pressed += OnRestartPressed;
		}

		if (_settingsButton != null)
		{
			_settingsButton.ProcessMode = ProcessModeEnum.Always;
			_settingsButton.Pressed += OnSettingsPressed;
		}
		else { GD.PrintErr("[PauseMenu Error] SettingsButton не найдена по пути CenterContainer/VBoxContainer/SettingsButton!"); }

		if (_quitButton != null)
		{
			_quitButton.ProcessMode = ProcessModeEnum.Always;
			_quitButton.Pressed += OnQuitPressed;
		}

		_menuButtons = new Button[] { _resumeButton, _restartButton, _settingsButton, _quitButton };

		_currentIndex = 0;
		_resumeButton?.GrabFocus();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged && Visible)
		{
			_currentIndex = 0;
			_resumeButton?.GrabFocus();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (_settingsPanel != null && _settingsPanel.Visible) return;
		if (!Visible) return;
		if (_menuButtons == null || _menuButtons.Length == 0) return;

		if (@event.IsActionPressed("ui_up"))
		{
			_currentIndex--;
			if (_currentIndex < 0) _currentIndex = _menuButtons.Length - 1;
			UpdateFocus();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			_currentIndex++;
			if (_currentIndex >= _menuButtons.Length) _currentIndex = 0;
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

	private void OnResumePressed()
	{
		GD.Print("[PauseMenu] Нажата кнопка Продолжить.");
		if (RunManager.Instance != null)
		{
			RunManager.Instance.TogglePause();
		}
	}

	private void OnRestartPressed()
	{
		GD.Print("[PauseMenu] Нажата кнопка Перезапуск.");
		GetTree().Paused = false;
		if (RunManager.Instance != null)
		{
			RunManager.Instance.RestartGame();
		}
	}

	private void OnSettingsPressed()
	{
		GD.Print("[PauseMenu] Нажата кнопка Настройки.");
		if (_settingsPanel != null)
		{
			// Запоминаем, что сейчас мы находимся на кнопке настроек (индекс 2 в нашем массиве: Resume, Restart, Settings, Quit)
			for (int i = 0; i < _menuButtons.Length; i++)
			{
				if (_menuButtons[i] == _settingsButton)
				{
					_currentIndex = i;
					break;
				}
			}

			_settingsPanel.Visible = true;
			
			var backButton = _settingsPanel.FindChild("BackButton", true, false) as Button;
			if (backButton != null)
			{
				backButton.GrabFocus();
				GD.Print("[PauseMenu] Фокус передан на BackButton в настройках.");
			}
		}
	}

	// Добавляем отслеживание: когда настройки закрываются, возвращаем фокус на сохраненную кнопку
	public override void _Process(double delta)
	{
		// Если настройки скрылись, а мы находились в меню паузы — проверяем фокус
		if (Visible && (_settingsPanel == null || !_settingsPanel.Visible))
		{
			// Если ни у одного элемента в паузе сейчас нет фокуса (или фокус ушел куда-то еще), возвращаем его
			var focusedNode = GetViewport().GuiGetFocusOwner();
			if (focusedNode == null || !IsAncestorOf(focusedNode))
			{
				UpdateFocus();
			}
		}
	}

	private void OnQuitPressed()
	{
		GD.Print("[PauseMenu] Нажата кнопка Выход.");
		GetTree().Paused = false;
		
		if (RunManager.Instance != null)
		{
			RunManager.Instance.EndRun();
		}

		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
}

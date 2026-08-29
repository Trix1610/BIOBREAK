using Godot;

public partial class PauseMenu : Control
{
	private Button _resumeButton;
	private Button _restartButton;
	private Button _quitButton;

	// Путь к твоему главному меню (настрой при необходимости)
	[Export(PropertyHint.File, "*.tscn")]
	private string MainMenuScenePath { get; set; } = "res://scenes/ui/MainMenu.tscn";

	public override void _Ready()
	{
		// Меню паузы должно работать даже тогда, когда игра на паузе!
		ProcessMode = ProcessModeEnum.Always;

		_resumeButton = GetNodeOrNull<Button>("VBoxContainer/ResumeButton");
		_restartButton = GetNodeOrNull<Button>("VBoxContainer/RestartButton");
		_quitButton = GetNodeOrNull<Button>("VBoxContainer/QuitButton");

		if (_resumeButton != null) _resumeButton.Pressed += OnResumePressed;
		if (_restartButton != null) _restartButton.Pressed += OnRestartPressed;
		if (_quitButton != null) _quitButton.Pressed += OnQuitPressed;
	}

	private void OnResumePressed()
	{
		// Вызываем метод закрытия паузы через RunManager
		if (RunManager.Instance != null)
		{
			RunManager.Instance.TogglePause();
		}
	}

	private void OnRestartPressed()
	{
		// Снимаем с паузы и перезапускаем забег
		GetTree().Paused = false;
		if (RunManager.Instance != null)
		{
			RunManager.Instance.RestartGame();
		}
	}

	private void OnQuitPressed()
	{
		// Снимаем паузу
		GetTree().Paused = false;
		
		// Выключаем флаг активности забега в RunManager
		if (RunManager.Instance != null)
		{
			RunManager.Instance.EndRun(); // добавим этот метод в RunManager, смотри шаг 3
		}

		// Переходим в главное меню
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
}

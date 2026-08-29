using Godot;

public partial class MainMenu : Control
{
	private Button _startButton;
	private Button _quitButton;

	[Export(PropertyHint.File, "*.tscn")] 
	private string GameScenePath { get; set; } = "res://scenes/main.tscn";

	public override void _Ready()
	{
		GD.Print("[MainMenu Debug] Метод _Ready() запущен! Главное меню успешно инициализировано.");

		// Проверяем, на месте ли пути к кнопкам
		_startButton = GetNodeOrNull<Button>("VBoxContainer/StartButton");
		_quitButton = GetNodeOrNull<Button>("VBoxContainer/QuitButton");

		if (_startButton != null)
		{
			GD.Print("[MainMenu Debug] Кнопка 'StartButton' успешно найдена.");
			_startButton.Pressed += OnStartPressed;
		}
		else
		{
			GD.PrintErr("[MainMenu Error] НЕ НАЙДЕНА кнопка по пути 'VBoxContainer/StartButton'!");
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
	}

	private void OnStartPressed()
	{
		GD.Print("[MainMenu Action] Нажата кнопка Старт. Запуск нового забега...");
		
		// Запускаем через RunManager, чтобы инициализировались сетка комнат и логика забега
		if (RunManager.Instance != null)
		{
			RunManager.Instance.StartNewRun();
		}
		else
		{
			// Запасной вариант, если RunManager вдруг не подключен как Autoload
			if (!string.IsNullOrEmpty(GameScenePath))
			{
				GetTree().ChangeSceneToFile(GameScenePath);
			}
		}
	}

	private void OnQuitPressed()
	{
		GD.Print("[MainMenu Action] Нажата кнопка Выход.");
		GetTree().Quit();
	}
}

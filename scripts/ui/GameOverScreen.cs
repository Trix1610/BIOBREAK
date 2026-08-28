using Godot;

public partial class GameOverScreen : Control
{
	private Button _restartButton;

	public override void _Ready()
	{
		// ВАЖНО: Укажи здесь точный путь к твоей кнопке рестарта внутри сцены GameOverScreen.
		// Если кнопка лежит прямо на корне, оставь "RestartButton". 
		// Если она лежит внутри VBoxContainer, напиши "VBoxContainer/RestartButton".
		_restartButton = GetNodeOrNull<Button>("VBoxContainer/RestartButton"); 

		if (_restartButton != null)
		{
			// Убеждаемся, что кнопка работает во время паузы
			_restartButton.ProcessMode = ProcessModeEnum.Always;
			
			// Подписываемся на событие нажатия
			_restartButton.Pressed += OnRestartPressed;
		}
		else
		{
			GD.PrintErr("GameOverScreen: Не удалось найти кнопку рестарта!");
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
			// Защитный вариант на случай ручного запуска
			GetTree().Paused = false;
			GetTree().ReloadCurrentScene();
		}
	}
}

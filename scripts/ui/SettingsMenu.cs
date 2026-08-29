using Godot;
using System.Collections.Generic;

public partial class SettingsMenu : Control
{
	private Button _backButton;
	private HSlider _masterVolumeSlider;
	private CheckButton _fullscreenToggle;
	private CheckButton _vsyncToggle;
	private OptionButton _resolutionOption;

	private Control[] _settingsElements;
	private int _currentIndex = 0;

	// Список разрешений, включая стандартные и Ultrawide мониторы
	private readonly List<Vector2I> _resolutions = new()
	{
		new Vector2I(1280, 720),
		new Vector2I(1366, 768),
		new Vector2I(1600, 900),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1080), // Ultrawide 21:9
		new Vector2I(2560, 1440), // 2K QHD
		new Vector2I(3440, 1440)  // Ultrawide 21:9 2K
	};

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_backButton = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/BackButton");
		_masterVolumeSlider = GetNodeOrNull<HSlider>("CenterContainer/VBoxContainer/MasterVolumeSlider");
		_fullscreenToggle = GetNodeOrNull<CheckButton>("CenterContainer/VBoxContainer/FullscreenToggle");
		_vsyncToggle = GetNodeOrNull<CheckButton>("CenterContainer/VBoxContainer/VsyncToggle");
		_resolutionOption = GetNodeOrNull<OptionButton>("CenterContainer/VBoxContainer/ResolutionOption");

		if (_backButton != null)
		{
			_backButton.ProcessMode = ProcessModeEnum.Always;
			_backButton.Pressed += OnBackPressed;
		}

		if (_masterVolumeSlider != null)
		{
			_masterVolumeSlider.ProcessMode = ProcessModeEnum.Always;
			float currentDb = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
			_masterVolumeSlider.Value = Mathf.DbToLinear(currentDb) * 100f;
			_masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
		}

		if (_fullscreenToggle != null)
		{
			_fullscreenToggle.ProcessMode = ProcessModeEnum.Always;
			_fullscreenToggle.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
			_fullscreenToggle.Toggled += OnFullscreenToggled;
		}

		if (_vsyncToggle != null)
		{
			_vsyncToggle.ProcessMode = ProcessModeEnum.Always;
			_vsyncToggle.ButtonPressed = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;
			_vsyncToggle.Toggled += OnVsyncToggled;
		}

		if (_resolutionOption != null)
		{
			_resolutionOption.ProcessMode = ProcessModeEnum.Always;
			SetupResolutions();
			_resolutionOption.ItemSelected += OnResolutionSelected;
		}

		// Порядок элементов: Разрешение теперь первое, затем Звук, Полноэкранный режим, VSync и Кнопка «Назад»
		_settingsElements = new Control[] { _resolutionOption, _masterVolumeSlider, _fullscreenToggle, _vsyncToggle, _backButton };
	}

	private void SetupResolutions()
	{
		_resolutionOption.Clear();
		Vector2I currentSize = DisplayServer.WindowGetSize();

		int selectedIndex = 0;
		for (int i = 0; i < _resolutions.Count; i++)
		{
			Vector2I res = _resolutions[i];
			_resolutionOption.AddItem($"{res.X} x {res.Y}");

			if (res == currentSize)
			{
				selectedIndex = i;
			}
		}
		_resolutionOption.Select(selectedIndex);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged && Visible)
		{
			_currentIndex = 0;
			CallDeferred(nameof(FocusCurrentElement));
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!Visible) return;
		if (_settingsElements == null || _settingsElements.Length == 0) return;

		if (@event.IsActionPressed("ui_up"))
		{
			_currentIndex--;
			if (_currentIndex < 0) _currentIndex = _settingsElements.Length - 1;
			FocusCurrentElement();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_down"))
		{
			_currentIndex++;
			if (_currentIndex >= _settingsElements.Length) _currentIndex = 0;
			FocusCurrentElement();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_cancel"))
		{
			OnBackPressed();
			GetViewport().SetInputAsHandled();
		}
	}

	private void FocusCurrentElement()
	{
		if (_currentIndex >= 0 && _currentIndex < _settingsElements.Length && _settingsElements[_currentIndex] != null)
		{
			_settingsElements[_currentIndex].GrabFocus();
		}
	}

	private void OnMasterVolumeChanged(double value)
	{
		int busIndex = AudioServer.GetBusIndex("Master");
		float db = Mathf.LinearToDb((float)value / 100f);
		if (value <= 0) db = -80f;
		AudioServer.SetBusVolumeDb(busIndex, db);
	}

	private void OnFullscreenToggled(bool isFullscreen)
	{
		if (isFullscreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}

		// Сохраняем выбор в файл через RunManager
		RunManager.SaveVideoSettings(isFullscreen);
	}

	private void OnVsyncToggled(bool isVsync)
	{
		if (isVsync)
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
		}
		else
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
		}
	}

	private void OnResolutionSelected(long index)
	{
		if (index >= 0 && index < _resolutions.Count)
		{
			Vector2I selectedRes = _resolutions[(int)index];
			
			if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen)
			{
				DisplayServer.WindowSetSize(selectedRes);
				Vector2I screenSize = DisplayServer.ScreenGetSize();
				DisplayServer.WindowSetPosition((screenSize - selectedRes) / 2);
			}
			
			GD.Print($"[SettingsMenu] Установлено разрешение: {selectedRes.X}x{selectedRes.Y}");
		}
	}

	private void OnBackPressed()
	{
		GD.Print("[SettingsMenu] Закрытие настроек.");
		Hide();
	}
}

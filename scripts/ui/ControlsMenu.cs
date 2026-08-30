using Godot;
using System.Collections.Generic;

public partial class ControlsMenu : Control
{
	private string[] _actionsToRebind = new string[] { "ui_left", "ui_right", "jump" };
	private string _actionCurrentlyRebinding = null;
	private int _eventIndexToRebind = 0;
	private Button _buttonCurrentlyRebinding = null;
	private List<Button> _allButtons = new();
	private int _currentRow = 0;
	private int _currentCol = 0;

	[Export] private VBoxContainer _rebindContainer;
	[Export] private Button _backButton;

	private const string SettingsFilePath = "user://keybindings.cfg";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		
		if (_backButton != null)
		{
			_backButton.Pressed += OnBackButtonPressed;
		}

		PopulateRebindList();
		LoadCustomControls();
		
		// Даем время на построение UI
		CallDeferred(nameof(GrabInitialFocus));
	}

	private void GrabInitialFocus()
	{
		UpdateButtonsList();
		_currentRow = 0;
		_currentCol = 0;
		FocusCurrentButton();
	}

	private void UpdateButtonsList()
	{
		_allButtons.Clear();
		
		// Собираем все кнопки в порядке: строка 1 (осн, альт), строка 2 (осн, альт), ...
		if (_rebindContainer != null)
		{
			foreach (Node child in _rebindContainer.GetChildren())
			{
				if (child is HBoxContainer row)
				{
					foreach (Node rowChild in row.GetChildren())
					{
						if (rowChild is Button btn)
						{
							_allButtons.Add(btn);
						}
					}
				}
			}
		}
		
		if (_backButton != null)
		{
			_allButtons.Add(_backButton);
		}
	}

	private void FocusCurrentButton()
	{
		int totalRows = _actionsToRebind.Length;
		
		// Если мы на кнопке "Назад"
		if (_currentRow >= totalRows)
		{
			if (_backButton != null)
			{
				_backButton.GrabFocus();
			}
			return;
		}
		
		int index = _currentRow * 2 + _currentCol;
		if (index >= 0 && index < _allButtons.Count)
		{
			_allButtons[index].GrabFocus();
		}
	}

	private void PopulateRebindList()
	{
		if (_rebindContainer == null) return;

		// Очищаем
		foreach (Node child in _rebindContainer.GetChildren())
		{
			child.QueueFree();
		}

		for (int i = 0; i < _actionsToRebind.Length; i++)
		{
			string action = _actionsToRebind[i];
			
			var row = new HBoxContainer();
			row.CustomMinimumSize = new Vector2(0, 40);

			var label = new Label();
			label.Text = GetActionDisplayName(action);
			label.CustomMinimumSize = new Vector2(180, 0);
			row.AddChild(label);

			// Две кнопки: основная и альтернативная
			for (int slot = 0; slot < 2; slot++)
			{
				var button = new Button();
				button.Text = GetKeyTextForIndex(action, slot);
				button.CustomMinimumSize = new Vector2(110, 0);

				string actionCopy = action;
				int slotCopy = slot;
				button.Pressed += () => StartRebinding(actionCopy, slotCopy, button);

				row.AddChild(button);
			}

			_rebindContainer.AddChild(row);
		}
		
		UpdateButtonsList();
	}

	private string GetActionDisplayName(string action)
	{
		return action switch
		{
			"ui_left" => "Движение влево",
			"ui_right" => "Движение вправо",
			"jump" => "Прыжок",
			_ => action
		};
	}

	private string GetKeyTextForIndex(string action, int index)
	{
		var events = InputMap.ActionGetEvents(action);
		List<InputEventKey> keyEvents = new();
		
		foreach (var ev in events)
		{
			if (ev is InputEventKey k) keyEvents.Add(k);
		}

		if (keyEvents.Count > index && keyEvents[index] != null)
		{
			var keyEvent = keyEvents[index];
			if (keyEvent.Keycode != Key.None)
			{
				return keyEvent.Keycode.ToString().ToUpper();
			}
			string label = keyEvent.AsTextKeyLabel();
			if (!string.IsNullOrEmpty(label)) return label;
		}
		return "ПУСТО";
	}

	private void StartRebinding(string actionName, int eventIndex, Button button)
	{
		if (_actionCurrentlyRebinding != null) return;

		_actionCurrentlyRebinding = actionName;
		_eventIndexToRebind = eventIndex;
		_buttonCurrentlyRebinding = button;
		button.Text = "...";
	}

	public override void _Input(InputEvent @event)
	{
		// Escape - отмена ребинда или выход
		if (@event.IsActionPressed("ui_cancel") || (@event is InputEventKey ek && ek.Pressed && ek.Keycode == Key.Escape))
		{
			if (_actionCurrentlyRebinding != null)
			{
				CancelRebinding();
				GetViewport().SetInputAsHandled();
				return;
			}
			else
			{
				OnBackButtonPressed();
				GetViewport().SetInputAsHandled();
				return;
			}
		}

		// Режим переназначения
		if (_actionCurrentlyRebinding != null)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				// Сохраняем новую клавишу
				var events = new List<InputEvent>(InputMap.ActionGetEvents(_actionCurrentlyRebinding));
				
				while (events.Count <= _eventIndexToRebind)
				{
					events.Add(new InputEventKey());
				}
				
				events[_eventIndexToRebind] = keyEvent;
				
				// Очищаем и перезаписываем
				foreach (var ev in InputMap.ActionGetEvents(_actionCurrentlyRebinding))
				{
					InputMap.ActionEraseEvent(_actionCurrentlyRebinding, ev);
				}
				
				foreach (var ev in events)
				{
					if (ev != null) InputMap.ActionAddEvent(_actionCurrentlyRebinding, ev);
				}
				
				// Обновляем текст кнопки
				if (_buttonCurrentlyRebinding != null)
				{
					_buttonCurrentlyRebinding.Text = keyEvent.Keycode != Key.None ? 
						keyEvent.Keycode.ToString().ToUpper() : 
						keyEvent.AsTextKeyLabel();
				}
				
				_actionCurrentlyRebinding = null;
				_buttonCurrentlyRebinding = null;
				
				SaveControls();
				GetViewport().SetInputAsHandled();
			}
			return;
		}

		// Навигация
		if (_allButtons.Count == 0) return;
		
		int totalRows = _actionsToRebind.Length;
		int maxRow = totalRows; // последняя строка — кнопка "Назад"

		// Вверх/вниз с зацикливанием
		if (@event.IsActionPressed("ui_down"))
		{
			_currentRow++;
			if (_currentRow > maxRow) _currentRow = 0; // Зацикливание: после "Назад" переходим на первую строку
			FocusCurrentButton();
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_up"))
		{
			_currentRow--;
			if (_currentRow < 0) _currentRow = maxRow; // Зацикливание: с первой строки переходим на "Назад"
			FocusCurrentButton();
			GetViewport().SetInputAsHandled();
		}
		// Влево/вправо — только для строк с клавишами
		else if (_currentRow < totalRows)
		{
			if (@event.IsActionPressed("ui_left"))
			{
				_currentCol = (_currentCol - 1 + 2) % 2;
				FocusCurrentButton();
				GetViewport().SetInputAsHandled();
			}
			else if (@event.IsActionPressed("ui_right"))
			{
				_currentCol = (_currentCol + 1) % 2;
				FocusCurrentButton();
				GetViewport().SetInputAsHandled();
			}
		}
		// Синхронизация при клике мышкой
		else if (@event is InputEventMouseButton)
		{
			for (int i = 0; i < _allButtons.Count; i++)
			{
				if (_allButtons[i].HasFocus())
				{
					_currentRow = i / 2;
					_currentCol = i % 2;
					break;
				}
			}
			// Проверяем, может фокус на кнопке "Назад"
			if (_backButton != null && _backButton.HasFocus())
			{
				_currentRow = totalRows;
				_currentCol = 0;
			}
		}
	}

	private void CancelRebinding()
	{
		if (_buttonCurrentlyRebinding != null && _actionCurrentlyRebinding != null)
		{
			_buttonCurrentlyRebinding.Text = GetKeyTextForIndex(_actionCurrentlyRebinding, _eventIndexToRebind);
		}
		_actionCurrentlyRebinding = null;
		_buttonCurrentlyRebinding = null;
	}

	private void OnBackButtonPressed()
	{
		var settingsPanel = GetParent().GetNodeOrNull<Control>("SettingsPanel");
		if (settingsPanel != null)
		{
			settingsPanel.Show();
			var controlsBtn = settingsPanel.GetNodeOrNull<Button>("CenterContainer/VBoxContainer/ControlsButton");
			controlsBtn?.GrabFocus();
		}
		QueueFree();
	}

	private void SaveControls()
	{
		var configFile = new ConfigFile();
		foreach (string action in _actionsToRebind)
		{
			var keycodes = new Godot.Collections.Array();
			foreach (var ev in InputMap.ActionGetEvents(action))
			{
				if (ev is InputEventKey keyEvent)
				{
					keycodes.Add((int)keyEvent.Keycode);
				}
			}
			configFile.SetValue("keybindings", action, keycodes);
		}
		configFile.Save(SettingsFilePath);
	}

	private void LoadCustomControls()
	{
		var configFile = new ConfigFile();
		if (configFile.Load(SettingsFilePath) != Error.Ok) return;

		foreach (string action in _actionsToRebind)
		{
			if (configFile.HasSectionKey("keybindings", action))
			{
				var keycodesVariant = configFile.GetValue("keybindings", action);
				if (keycodesVariant.Obj is Godot.Collections.Array keycodes)
				{
					// Очищаем старые
					foreach (var ev in InputMap.ActionGetEvents(action))
					{
						InputMap.ActionEraseEvent(action, ev);
					}
					
					// Загружаем новые
					foreach (var kc in keycodes)
					{
						int codeInt = (int)kc;
						if (codeInt != 0)
						{
							var keyEvent = new InputEventKey();
							keyEvent.Keycode = (Key)codeInt;
							InputMap.ActionAddEvent(action, keyEvent);
						}
					}
				}
			}
		}
		UpdateAllButtonTexts();
	}

	private void UpdateAllButtonTexts()
	{
		if (_rebindContainer == null) return;

		int actionIndex = 0;
		foreach (Node child in _rebindContainer.GetChildren())
		{
			if (child is HBoxContainer row && actionIndex < _actionsToRebind.Length)
			{
				string action = _actionsToRebind[actionIndex];
				int slot = 0;
				foreach (Node rowChild in row.GetChildren())
				{
					if (rowChild is Button btn)
					{
						btn.Text = GetKeyTextForIndex(action, slot);
						slot++;
					}
				}
				actionIndex++;
			}
		}
	}
}

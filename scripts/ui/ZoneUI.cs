using Godot;

public partial class ZoneUI : CanvasLayer
{
	private Label _zoneLabel;

	public override void _Ready()
	{
		AddToGroup("ZoneUI");

		_zoneLabel = GetNodeOrNull<Label>("ZoneLabel");
		if (_zoneLabel == null)
		{
			// Создаем Label если его нет
			_zoneLabel = new Label();
			_zoneLabel.Name = "ZoneLabel";
			_zoneLabel.Position = new Vector2(20, 20);
			_zoneLabel.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
			_zoneLabel.AddThemeColorOverride("font_color", Colors.White);
			_zoneLabel.AddThemeFontSizeOverride("font_size", 24);
			AddChild(_zoneLabel);
		}

		UpdateZoneDisplay();
	}

	public void UpdateZoneDisplay()
	{
		if (_zoneLabel == null) return;

		var currentZone = RunManager.Instance?.CurrentBioZone;
		if (currentZone != null && currentZone.ThemeData != null)
		{
			_zoneLabel.Text = currentZone.ThemeData.ThemeName;
		}
		else
		{
			_zoneLabel.Text = "Unknown Zone";
		}
	}
}

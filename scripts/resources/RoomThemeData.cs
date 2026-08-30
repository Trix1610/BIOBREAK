using Godot;

[GlobalClass]
public partial class RoomThemeData : Resource
{
	[Export] public string ThemeName { get; set; } = "Stomach";
	[Export] public Texture2D WallTexture { get; set; }
	[Export] public Color PlatformColor { get; set; } = new Color(0.18f, 0.2f, 0.28f);
	[Export] public Color BorderColor { get; set; } = new Color(0.4f, 0.6f, 0.9f);
}

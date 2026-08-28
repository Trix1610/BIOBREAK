using Godot;

[GlobalClass]
public partial class WeaponData : Resource
{
	[Export] public string WeaponName { get; set; } = "Пистолет";
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public Texture2D WeaponSprite { get; set; }
	[Export] public float FireRate { get; set; } = 0.3f;
	[Export] public float BulletSpeed { get; set; } = 550.0f;
	[Export] public int Damage { get; set; } = 20;
}

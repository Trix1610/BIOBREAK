using Godot;

[GlobalClass]
public partial class WeaponData : Resource
{
	[Export] public string WeaponName { get; set; } = "Пистолет";
	[Export] public string WeaponClass { get; set; } = "SingleShotWeapon";
	// Возможные значения: SingleShotWeapon, AutomaticWeapon, ShotgunWeapon, LaserWeapon, RailgunWeapon, SonicWeapon, ExplosiveWeapon
	
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public Texture2D WeaponSprite { get; set; }
	[Export] public float FireRate { get; set; } = 0.3f;
	[Export] public float BulletSpeed { get; set; } = 550.0f;
	[Export] public int Damage { get; set; } = 20;

	// Параметры для дробовика
	[Export] public int PelletCount { get; set; } = 1;
	[Export] public float SpreadAngle { get; set; } = 0f;
	[Export] public float RandomSpread { get; set; } = 0f;

	// Параметры для лучевого и площадного оружия
	[Export] public float Range { get; set; } = 1000f;
	[Export] public float AreaRadius { get; set; } = 200f;
	[Export] public bool CanPenetrate { get; set; } = false;

	// Параметры для взрывного оружия
	[Export] public float ExplosionDelay { get; set; } = 0f;
}

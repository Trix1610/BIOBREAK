using Godot;

public partial class ShotgunWeapon : ProjectileWeapon
{
	[Export] public int PelletCount { get; set; } = 5;
	[Export] public float SpreadAngle { get; set; } = 15f;
	[Export] public float RandomSpread { get; set; } = 5f;

	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		base._Ready();
		_rng.Randomize();
	}

	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		float totalSpread = SpreadAngle;
		float angleStep = PelletCount > 1 ? totalSpread / (PelletCount - 1) : 0f;
		float startAngle = -totalSpread / 2f;

		for (int i = 0; i < PelletCount; i++)
		{
			float baseAngle = startAngle + (i * angleStep);
			float randomOffset = _rng.RandfRange(-RandomSpread, RandomSpread);
			float finalAngle = baseAngle + randomOffset;

			SpawnBullet(spawnPosition, targetPosition, finalAngle);
		}
	}
}

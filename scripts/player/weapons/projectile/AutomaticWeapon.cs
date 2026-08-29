using Godot;

public partial class AutomaticWeapon : ProjectileWeapon
{
	[Export] public bool IsFullAuto { get; set; } = true;
	
	private bool _isFiring = false;
	private Vector2 _lastTargetPosition;

	public override void Shoot(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (Data == null || _fireCooldown > 0) return;

		_lastTargetPosition = targetPosition;
		
		if (!IsFullAuto)
		{
			SpawnBullet(spawnPosition, targetPosition, 0f);
			return;
		}

		_isFiring = true;
		SpawnBullet(spawnPosition, targetPosition, 0f);
	}

	public void StopFiring()
	{
		_isFiring = false;
	}

	public void ContinueFiring(Vector2 spawnPosition, Vector2 targetPosition)
	{
		if (_isFiring && IsFullAuto && _fireCooldown <= 0)
		{
			_lastTargetPosition = targetPosition;
			SpawnBullet(spawnPosition, _lastTargetPosition, 0f);
		}
	}
}

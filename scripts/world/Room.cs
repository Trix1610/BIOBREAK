using Godot;

public partial class Room : Node2D
{
	[Export] public bool IsLeftDoorClosed { get; set; } = false;
	[Export] public bool IsRightDoorClosed { get; set; } = false;

	private PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
	private bool _isTransitioning = false;

	public override void _Ready()
	{
		// 1. Находим или спавним игрока
		Node2D player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		if (player == null && _playerScene != null)
		{
			player = _playerScene.Instantiate<Node2D>();
			AddChild(player);
		}

		// 2. Перемещаем к точке спавна
		string targetSpawnName = GameManager.Instance?.TargetSpawnPoint;
		if (!string.IsNullOrEmpty(targetSpawnName))
		{
			Marker2D spawnPoint = GetNodeOrNull<Marker2D>(targetSpawnName);
			if (spawnPoint != null && player != null)
			{
				player.GlobalPosition = spawnPoint.GlobalPosition;
			}
		}

		SetupDoors();
	}

	private void SetupDoors()
	{
		var leftDoor = GetNodeOrNull<Area2D>("LeftDoor");
		var rightDoor = GetNodeOrNull<Area2D>("RightDoor");

		if (leftDoor != null)
		{
			if (IsLeftDoorClosed)
			{
				leftDoor.Monitoring = false;
				leftDoor.Visible = false;
			}
			else
			{
				leftDoor.BodyEntered += OnLeftDoorEntered;
			}
		}

		if (rightDoor != null)
		{
			if (IsRightDoorClosed)
			{
				rightDoor.Monitoring = false;
				rightDoor.Visible = false;
			}
			else
			{
				rightDoor.BodyEntered += OnRightDoorEntered;
			}
		}
	}

	// ОБРАБОТЧИК ЛЕВОЙ ДВЕРИ (только 1 раз)
	private void OnLeftDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning)
		{
			_isTransitioning = true;
			RunManager.Instance?.MoveToRoom(new Vector2I(-1, 0), "SpawnRight");
		}
	}

	// ОБРАБОТЧИК ПРАВОЙ ДВЕРИ (только 1 раз)
	private void OnRightDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning)
		{
			_isTransitioning = true;
			RunManager.Instance?.MoveToRoom(new Vector2I(1, 0), "SpawnLeft");
		}
	}
}

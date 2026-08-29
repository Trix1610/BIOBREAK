using Godot;
using System;

public partial class DoorController : Node
{
	[Export] public bool IsLeftDoorClosed { get; set; } = false;
	[Export] public bool IsRightDoorClosed { get; set; } = false;
	[Export] public int LimitRight { get; set; } = 1920;

	private Area2D _leftDoor;
	private Area2D _rightDoor;
	private bool _isBattleActive = false;
	private bool _isTransitioning = false;

	public event Action OnRoomTransitionRequested;

	public void SetupDoorsAndSpawns(Node2D parent)
	{
		float rightWallX = LimitRight;

		// --- ПРАВАЯ СТОРОНА ---
		var rightDoor = parent.GetNodeOrNull<Area2D>("RightDoor");
		if (rightDoor != null)
		{
			var pos = rightDoor.GlobalPosition;
			pos.X = rightWallX - 40;
			rightDoor.GlobalPosition = pos;
		}

		var spawnRight = parent.GetNodeOrNull<Marker2D>("SpawnRight");
		if (spawnRight != null)
		{
			var pos = spawnRight.GlobalPosition;
			pos.X = rightWallX - 100;
			spawnRight.GlobalPosition = pos;
		}

		// --- ЛЕВАЯ СТОРОНА ---
		var leftDoor = parent.GetNodeOrNull<Area2D>("LeftDoor");
		if (leftDoor != null)
		{
			var pos = leftDoor.GlobalPosition;
			pos.X = 40;
			leftDoor.GlobalPosition = pos;
		}

		var spawnLeft = parent.GetNodeOrNull<Marker2D>("SpawnLeft");
		if (spawnLeft != null)
		{
			var pos = spawnLeft.GlobalPosition;
			pos.X = 100;
			spawnLeft.GlobalPosition = pos;
		}
	}

	public void InitializeDoors(Node2D parent)
	{
		_leftDoor = parent.GetNodeOrNull<Area2D>("LeftDoor");
		_rightDoor = parent.GetNodeOrNull<Area2D>("RightDoor");

		var currentRoomNode = RunManager.Instance?.CurrentRoom;

		bool hasLeftNeighbor = currentRoomNode != null && currentRoomNode.Neighbors.ContainsKey(new Vector2I(-1, 0));
		bool hasRightNeighbor = currentRoomNode != null && currentRoomNode.Neighbors.ContainsKey(new Vector2I(1, 0));

		if (_leftDoor != null)
		{
			if (IsLeftDoorClosed || !hasLeftNeighbor)
			{
				_leftDoor.Monitoring = false;
				_leftDoor.Visible = false;
			}
			else
			{
				_leftDoor.BodyEntered += OnLeftDoorEntered;
			}
		}

		if (_rightDoor != null)
		{
			if (IsRightDoorClosed || !hasRightNeighbor)
			{
				_rightDoor.Monitoring = false;
				_rightDoor.Visible = false;
			}
			else
			{
				_rightDoor.BodyEntered += OnRightDoorEntered;
			}
		}
	}

	public void TemporarilyDisableDoors()
	{
		if (_leftDoor != null) _leftDoor.Monitoring = false;
		if (_rightDoor != null) _rightDoor.Monitoring = false;
	}

	public void EnableDoorsAfterDelay()
	{
		if (_leftDoor != null && !IsLeftDoorClosed && !_isBattleActive)
			_leftDoor.Monitoring = true;
		if (_rightDoor != null && !IsRightDoorClosed && !_isBattleActive)
			_rightDoor.Monitoring = true;
	}

	public void LockDoors()
	{
		_isBattleActive = true;
		if (_leftDoor != null) _leftDoor.Monitoring = false;
		if (_rightDoor != null) _rightDoor.Monitoring = false;
	}

	public void UnlockDoors()
	{
		_isBattleActive = false;
		if (RunManager.Instance?.CurrentRoom != null)
		{
			RunManager.Instance.CurrentRoom.IsCleared = true;
		}

		if (_leftDoor != null && !IsLeftDoorClosed)
		{
			_leftDoor.Monitoring = true;
			CheckDoorOverlap(_leftDoor, OnLeftDoorEntered);
		}

		if (_rightDoor != null && !IsRightDoorClosed)
		{
			_rightDoor.Monitoring = true;
			CheckDoorOverlap(_rightDoor, OnRightDoorEntered);
		}
	}

	private void CheckDoorOverlap(Area2D door, Action<Node2D> onEntered)
	{
		var bodies = door.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body.IsInGroup("Player"))
			{
				onEntered(body);
				break;
			}
		}
	}

	private void OnLeftDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning && !_isBattleActive)
		{
			Vector2I targetDir = new Vector2I(-1, 0);
			if (CanMoveToDirection(targetDir))
			{
				_isTransitioning = true;
				RunManager.Instance?.MoveToRoom(targetDir, "SpawnRight");
				OnRoomTransitionRequested?.Invoke();
			}
		}
	}

	private void OnRightDoorEntered(Node2D body)
	{
		if (body.IsInGroup("Player") && !_isTransitioning && !_isBattleActive)
		{
			Vector2I targetDir = new Vector2I(1, 0);
			if (CanMoveToDirection(targetDir))
			{
				_isTransitioning = true;
				RunManager.Instance?.MoveToRoom(targetDir, "SpawnLeft");
				OnRoomTransitionRequested?.Invoke();
			}
		}
	}

	private bool CanMoveToDirection(Vector2I dir)
	{
		if (RunManager.Instance?.CurrentRoom == null) return false;
		Vector2I targetGridPos = RunManager.Instance.CurrentRoom.GridPos + dir;
		return RunManager.Instance.RoomGrid.ContainsKey(targetGridPos);
	}

	public bool IsBattleActive => _isBattleActive;
}

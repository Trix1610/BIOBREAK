using Godot;

public enum TransitionDirection
{
	Left,
	Right,
	Up,
	Down
}

public partial class LevelTransition : Area2D
{
	[Export] public TransitionDirection Direction { get; set; } = TransitionDirection.Right;
	[Export] public string TargetSpawnPoint { get; set; } = "SpawnLeft";

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			CallDeferred(nameof(PerformTransition));
		}
	}

	private void PerformTransition()
	{
		Vector2I gridDir = Direction switch
		{
			TransitionDirection.Left => Vector2I.Left,
			TransitionDirection.Right => Vector2I.Right,
			TransitionDirection.Up => Vector2I.Up,
			TransitionDirection.Down => Vector2I.Down,
			_ => Vector2I.Right
		};

		if (RunManager.Instance != null)
		{
			RunManager.Instance.MoveToRoom(gridDir, TargetSpawnPoint);
		}
	}
}

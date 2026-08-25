using Godot;

public partial class MainRoom : Node2D
{
	public override void _Ready()
	{
		// Only start a new run grid if the run is not active yet
		if (RunManager.Instance != null && !RunManager.Instance.IsRunActive)
		{
			RunManager.Instance.StartNewRun(7);
		}
	}
}

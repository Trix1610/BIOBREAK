using Godot;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	// Имя точки спавна, возле которой должен появиться игрок
	public string TargetSpawnPoint { get; set; } = "SpawnLeft";

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}
}

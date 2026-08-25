using Godot;
using System.Collections.Generic;

public class RoomNode
{
	public Vector2I GridPos { get; set; }
	public PackedScene RoomScene { get; set; }
	public bool IsVisited { get; set; } = false;

	// Словарь соседних комнат (направление -> узел комнаты)
	public Dictionary<Vector2I, RoomNode> Neighbors { get; set; } = new Dictionary<Vector2I, RoomNode>();

	// КОНСТРУКТОР С 2 АРГУМЕНТАМИ
	public RoomNode(Vector2I gridPos, PackedScene roomScene)
	{
		GridPos = gridPos;
		RoomScene = roomScene;
	}
}

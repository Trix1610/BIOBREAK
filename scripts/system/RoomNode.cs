using Godot;
using System.Collections.Generic;

public class RoomNode
{
	public Vector2I GridPos { get; set; }
	public PackedScene RoomScene { get; set; }
	public bool IsVisited { get; set; } = false;
	public bool IsCleared { get; set; } = false; // Новое поле: очищена ли комната

	public Dictionary<Vector2I, RoomNode> Neighbors { get; set; } = new Dictionary<Vector2I, RoomNode>();

	public RoomNode(Vector2I gridPos, PackedScene roomScene)
	{
		GridPos = gridPos;
		RoomScene = roomScene;
	}
}

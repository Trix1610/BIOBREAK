using Godot;
using System.Collections.Generic;

public class BioZone
{
	public RoomTheme Theme { get; set; }
	public List<RoomNode> Rooms { get; set; }
	public RoomThemeData ThemeData { get; set; }

	public BioZone(RoomTheme theme, RoomThemeData themeData)
	{
		Theme = theme;
		ThemeData = themeData;
		Rooms = new List<RoomNode>();
	}

	public bool IsCleared()
	{
		foreach (var room in Rooms)
		{
			if (!room.IsCleared)
				return false;
		}
		return true;
	}

	public void AddRoom(RoomNode room)
	{
		Rooms.Add(room);
	}
}

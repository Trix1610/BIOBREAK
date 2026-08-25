using Godot;
using System.Collections.Generic;

public partial class Minimap : Control
{
	[Export] public Vector2 CellSize = new Vector2(24, 24);
	[Export] public Vector2 CellMargin = new Vector2(8, 8); // Увеличили отступ для линий
	[Export] public Color LineColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
	[Export] public float LineWidth = 3.0f;

	public override void _Process(double delta)
	{
		// Перерисовываем UI каждый кадр
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (RunManager.Instance == null || RunManager.Instance.RoomGrid == null)
			return;

		Vector2 centerOffset = Size / 2;

		// ШАГ 1: Сначала рисуем все соединительные линии (чтобы они были ПОД квадратами комнат)
		foreach (var pair in RunManager.Instance.RoomGrid)
		{
			Vector2I gridPos = pair.Key;
			RoomNode room = pair.Value;

			if (!room.IsVisited)
				continue;

			Vector2 roomCenter = GetCanvasPosition(gridPos, centerOffset);

			foreach (var neighborPair in room.Neighbors)
			{
				Vector2I direction = neighborPair.Key;
				RoomNode neighborRoom = neighborPair.Value;

				// Рисуем связь только если соседняя комната тоже посещена
				if (neighborRoom != null && neighborRoom.IsVisited)
				{
					Vector2 neighborCenter = GetCanvasPosition(neighborRoom.GridPos, centerOffset);
					DrawLine(roomCenter, neighborCenter, LineColor, LineWidth);
				}
			}
		}

		// ШАГ 2: Рисуем сами квадраты комнат
		foreach (var pair in RunManager.Instance.RoomGrid)
		{
			Vector2I gridPos = pair.Key;
			RoomNode room = pair.Value;

			if (!room.IsVisited)
				continue;

			Vector2 drawPos = GetCanvasPosition(gridPos, centerOffset);
			Rect2 roomRect = new Rect2(drawPos - CellSize / 2, CellSize);

			// Окраска: Зеленая — текущая комната, Серая — посещенная
			Color roomColor = (room == RunManager.Instance.CurrentRoom)
				? new Color(0.2f, 0.9f, 0.3f, 0.9f)
				: new Color(0.4f, 0.4f, 0.45f, 0.8f);

			// Заливка квадрата
			DrawRect(roomRect, roomColor);
			
			// Белая обводка
			DrawRect(roomRect, Colors.White, false, 1.5f);
		}
	}

	// Вспомогательный метод для расчета UI-координат из координат сетки
	private Vector2 GetCanvasPosition(Vector2I gridPos, Vector2 centerOffset)
	{
		return centerOffset + new Vector2(
			gridPos.X * (CellSize.X + CellMargin.X),
			gridPos.Y * (CellSize.Y + CellMargin.Y)
		);
	}
}

using Godot;
using System.Collections.Generic;

public partial class Minimap : Control
{
	// Объявляем Instance ровно один раз
	public static Minimap Instance { get; private set; }

	[Export] public Vector2 CellSize = new Vector2(24, 24);
	[Export] public Vector2 CellMargin = new Vector2(8, 8);
	[Export] public Color LineColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
	[Export] public float LineWidth = 3.0f;

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public new void SetVisible(bool isVisible)
	{
		Visible = isVisible;
	}

	public override void _Process(double delta)
	{
		// Перерисовываем миникарту каждый кадр для актуализации состояния
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (RunManager.Instance == null || RunManager.Instance.RoomGrid == null)
			return;

		Vector2 centerOffset = Size / 2;

		// ШАГ 1: Отрисовка соединительных линий (коридоров) под квадратами комнат
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

				// Рисуем переход только если соседняя комната тоже была посещена
				if (neighborRoom != null && neighborRoom.IsVisited)
				{
					Vector2 neighborCenter = GetCanvasPosition(neighborRoom.GridPos, centerOffset);
					DrawLine(roomCenter, neighborCenter, LineColor, LineWidth);
				}
			}
		}

		// ШАГ 2: Отрисовка самих квадратов комнат и их рамок
		foreach (var pair in RunManager.Instance.RoomGrid)
		{
			Vector2I gridPos = pair.Key;
			RoomNode room = pair.Value;

			if (!room.IsVisited)
				continue;

			Vector2 drawPos = GetCanvasPosition(gridPos, centerOffset);
			Rect2 roomRect = new Rect2(drawPos - CellSize / 2, CellSize);

			// Основной цвет заливочного квадрата:
			// Зеленый — текущая комната, Серый — пройденная/посещенная
			Color roomColor = (room == RunManager.Instance.CurrentRoom)
				? new Color(0.2f, 0.9f, 0.3f, 0.9f)
				: new Color(0.4f, 0.4f, 0.45f, 0.8f);

			// Цвет рамки зависит от состояния зачистки (IsCleared):
			// Красный — идет бой, Желтый — текущая комната зачищена, Белый/Розоватый — остальные
			Color strokeColor = Colors.White;

			if (room == RunManager.Instance.CurrentRoom)
			{
				strokeColor = room.IsCleared ? Colors.Yellow : Colors.Red;
			}
			else if (!room.IsCleared)
			{
				strokeColor = new Color(0.9f, 0.3f, 0.3f);
			}

			// Отрисовка квадрата и рамки
			DrawRect(roomRect, roomColor);
			DrawRect(roomRect, strokeColor, false, 1.5f);
		}
	}

	// Вспомогательный метод расчёта экранных координат из логических (GridPos)
	private Vector2 GetCanvasPosition(Vector2I gridPos, Vector2 centerOffset)
	{
		return centerOffset + new Vector2(
			gridPos.X * (CellSize.X + CellMargin.X),
			gridPos.Y * (CellSize.Y + CellMargin.Y)
		);
	}
}

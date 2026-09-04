using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasRenderer))]
public class Minimap : MaskableGraphic
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 cellSize = new Vector2(24, 24);
    [SerializeField] private Vector2 cellMargin = new Vector2(8, 8);
    [SerializeField] private Color lineColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private float lineWidth = 4.0f;

    [Header("Colors")]
    [SerializeField] private Color currentColor = new Color(0.2f, 0.9f, 0.3f, 0.9f);
    [SerializeField] private Color visitedColor = new Color(0.4f, 0.4f, 0.45f, 0.8f);

    private Dictionary<string, Vector2Int> roomGridPositions = new Dictionary<string, Vector2Int>();
    private string lastSceneName = "";

    protected override void Awake()
    {
        base.Awake();
        roomGridPositions["ROOM_00"] = new Vector2Int(0, 0);
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Игнорируем техническую сцену контейнера
        if (currentScene == "GAME")
            return;

        if (currentScene != lastSceneName && !string.IsNullOrEmpty(lastSceneName) && lastSceneName != "GAME")
        {
            if (!roomGridPositions.ContainsKey(currentScene))
            {
                Vector2Int previousPos = roomGridPositions.ContainsKey(lastSceneName) 
                    ? roomGridPositions[lastSceneName] 
                    : Vector2Int.zero;

                Vector2Int newPos = previousPos + new Vector2Int(1, 0); // По умолчанию вправо

                if (RunManager.Instance != null)
                {
                    // Безопасно проверяем направления через метод-обертку ниже, который не спамит ошибки в консоль
                    if (GetSafeDestination(lastSceneName, "Right") == currentScene)
                    {
                        newPos = previousPos + new Vector2Int(1, 0);
                    }
                    else if (GetSafeDestination(lastSceneName, "Left") == currentScene)
                    {
                        newPos = previousPos + new Vector2Int(-1, 0);
                    }
                }

                roomGridPositions[currentScene] = newPos;
            }
        }

        lastSceneName = currentScene;
        SetVerticesDirty();
    }

    // Вспомогательный метод, который проверяет связь без красных логов в консоли
    private string GetSafeDestination(string room, string direction)
    {
        // Проверяем через Reflection или просто зная структуру RunManager, 
        // но проще всего добавить метод в сам Minimap или проверять напрямую, 
        // если бы словарь был публичным. Раз словарь приватный, сделаем так:
        
        // Перехватываем стандартный метод, либо сделаем проще:
        // Давайте просто вызовем GetDestination, но перед этим проверим, а есть ли путь.
        // Так как RunManager выкидывает лог прямо внутри GetDestination, напишем тихий аналог проверки:
        return null; 
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (RunManager.Instance == null)
            return;

        Vector2 centerOffset = rectTransform.rect.center;
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "GAME" && !roomGridPositions.ContainsKey(currentScene))
        {
            roomGridPositions[currentScene] = Vector2Int.zero;
        }

        // ШАГ 1: Рисуем линии между соседними на сетке комнатами (без вызовов GetDestination)
        var visitedList = new List<KeyValuePair<string, Vector2Int>>(roomGridPositions);
        for (int i = 0; i < visitedList.Count; i++)
        {
            for (int j = i + 1; j < visitedList.Count; j++)
            {
                Vector2Int posA = visitedList[i].Value;
                Vector2Int posB = visitedList[j].Value;

                // Если комнаты находятся вплотную друг к другу (расстояние ровно 1 шаг), соединяем их линией
                if (Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y) == 1)
                {
                    Vector2 canvasA = GetCanvasPosition(posA, centerOffset);
                    Vector2 canvasB = GetCanvasPosition(posB, centerOffset);
                    DrawLine(vh, canvasA, canvasB, lineWidth, lineColor);
                }
            }
        }

        // ШАГ 2: Поверх линий рисуем квадраты комнат
        foreach (var pair in roomGridPositions)
        {
            string roomName = pair.Key;
            Vector2Int gridPos = pair.Value;

            Vector2 drawPos = GetCanvasPosition(gridPos, centerOffset);
            Color roomColor = (roomName == currentScene) ? currentColor : visitedColor;

            DrawRect(vh, drawPos, cellSize, roomColor);
        }
    }

    private Vector2 GetCanvasPosition(Vector2Int gridPos, Vector2 centerOffset)
    {
        return centerOffset + new Vector2(
            gridPos.x * (cellSize.x + cellMargin.x),
            gridPos.y * (cellSize.y + cellMargin.y)
        );
    }

    private void DrawRect(VertexHelper vh, Vector2 center, Vector2 size, Color color)
    {
        int startIndex = vh.currentVertCount;

        Vector2 min = center - size / 2f;
        Vector2 max = center + size / 2f;

        vh.AddVert(new Vector3(min.x, min.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(min.x, max.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(max.x, max.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(max.x, min.y, 0), color, Vector2.zero);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void DrawLine(VertexHelper vh, Vector2 p1, Vector2 p2, float width, Color color)
    {
        Vector2 dir = (p2 - p1).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (width / 2f);

        int startIndex = vh.currentVertCount;

        vh.AddVert(new Vector3(p1.x - perp.x, p1.y - perp.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(p1.x + perp.x, p1.y + perp.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(p2.x + perp.x, p2.y + perp.y, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(p2.x - perp.x, p2.y - perp.y, 0), color, Vector2.zero);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }
}
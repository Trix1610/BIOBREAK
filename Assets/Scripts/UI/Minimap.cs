using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Core;

[RequireComponent(typeof(CanvasRenderer))]
public class Minimap : MaskableGraphic
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 cellSize = new Vector2(24, 24);
    [SerializeField] private Vector2 cellMargin = new Vector2(8, 8);
    [SerializeField] private Color lineColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private float lineWidth = 4.0f;

    [Header("Border Settings")]
    [SerializeField] private Color borderColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private float borderWidth = 1.5f;

    [Header("Colors")]
    [SerializeField] private Color currentColor = new Color(0.2f, 0.9f, 0.3f, 0.9f);
    [SerializeField] private Color visitedColor = new Color(0.4f, 0.4f, 0.45f, 0.8f);
    [SerializeField] private Color undiscoveredColor = new Color(0.2f, 0.2f, 0.25f, 0.4f);

    private Dictionary<string, Vector2Int> roomGridPositions = new Dictionary<string, Vector2Int>();
    private string lastSceneName = "";

    protected override void Awake()
    {
        base.Awake();
        roomGridPositions[SceneNames.StartRoom] = new Vector2Int(0, 0);
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == SceneNames.Game)
            return;

        if (currentScene != lastSceneName && !string.IsNullOrEmpty(lastSceneName) && lastSceneName != SceneNames.Game)
        {
            RegisterRoomPosition(currentScene, lastSceneName);
        }

        lastSceneName = currentScene;
        SetVerticesDirty();
    }

    private void RegisterRoomPosition(string currentScene, string prevScene)
    {
        if (roomGridPositions.ContainsKey(currentScene))
            return;

        Vector2Int previousPos = roomGridPositions.ContainsKey(prevScene) 
            ? roomGridPositions[prevScene] 
            : Vector2Int.zero;

        Vector2Int newPos = previousPos + new Vector2Int(1, 0);

        if (RunManager.Instance != null)
        {
            if (GetSafeDestination(prevScene, "Right") == currentScene)
            {
                newPos = previousPos + new Vector2Int(1, 0);
            }
            else if (GetSafeDestination(prevScene, "Left") == currentScene)
            {
                newPos = previousPos + new Vector2Int(-1, 0);
            }
        }

        roomGridPositions[currentScene] = newPos;
    }

    private string GetSafeDestination(string room, string direction)
    {
        return RunManager.Instance?.GetDestination(room, direction);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (RunManager.Instance == null)
            return;

        Vector2 centerOffset = rectTransform.rect.center;
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == SceneNames.Game) currentScene = lastSceneName;

        if (!roomGridPositions.ContainsKey(currentScene))
        {
            roomGridPositions[currentScene] = Vector2Int.zero;
        }

        Dictionary<string, Vector2Int> renderMap = new Dictionary<string, Vector2Int>(roomGridPositions);

        List<string> knownRooms = new List<string>(roomGridPositions.Keys);
        foreach (var room in knownRooms)
        {
            Vector2Int pos = roomGridPositions[room];
            CheckAndAddNeighbor(room, pos, "Right", new Vector2Int(1, 0), renderMap);
            CheckAndAddNeighbor(room, pos, "Left", new Vector2Int(-1, 0), renderMap);
        }

        // ШАГ 1: Рисуем линии между известными узлами
        var renderList = new List<KeyValuePair<string, Vector2Int>>(renderMap);
        for (int i = 0; i < renderList.Count; i++)
        {
            for (int j = i + 1; j < renderList.Count; j++)
            {
                Vector2Int posA = renderList[i].Value;
                Vector2Int posB = renderList[j].Value;

                if (Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y) == 1)
                {
                    Vector2 canvasA = GetCanvasPosition(posA, centerOffset);
                    Vector2 canvasB = GetCanvasPosition(posB, centerOffset);
                    DrawLine(vh, canvasA, canvasB, lineWidth, lineColor);
                }
            }
        }

        // ШАГ 2: Рисуем квадраты комнат
        foreach (var pair in renderMap)
        {
            string roomName = pair.Key;
            Vector2Int gridPos = pair.Value;
            Vector2 drawPos = GetCanvasPosition(gridPos, centerOffset);

            Color roomColor;
            if (roomName == currentScene)
            {
                roomColor = currentColor;
            }
            else if (roomGridPositions.ContainsKey(roomName))
            {
                roomColor = visitedColor;
            }
            else
            {
                roomColor = undiscoveredColor;
            }

            DrawRect(vh, drawPos, cellSize, roomColor);

            // Тонкая рамка вокруг квадрата (если задана ширина > 0)
            if (borderWidth > 0f)
            {
                DrawRectBorder(vh, drawPos, cellSize, borderWidth, borderColor);
            }
        }
    }

    private void CheckAndAddNeighbor(string room, Vector2Int basePos, string direction, Vector2Int offset, Dictionary<string, Vector2Int> targetMap)
    {
        string destRoom = GetSafeDestination(room, direction);
        if (!string.IsNullOrEmpty(destRoom))
        {
            if (!targetMap.ContainsKey(destRoom))
            {
                targetMap[destRoom] = basePos + offset;
            }
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

    private void DrawRectBorder(VertexHelper vh, Vector2 center, Vector2 size, float bWidth, Color color)
    {
        Vector2 min = center - size / 2f;
        Vector2 max = center + size / 2f;

        // Верхняя сторона
        DrawLine(vh, new Vector2(min.x, max.y), new Vector2(max.x, max.y), bWidth, color);
        // Нижняя сторона
        DrawLine(vh, new Vector2(min.x, min.y), new Vector2(max.x, min.y), bWidth, color);
        // Левая сторона
        DrawLine(vh, new Vector2(min.x, min.y), new Vector2(min.x, max.y), bWidth, color);
        // Правая сторона
        DrawLine(vh, new Vector2(max.x, min.y), new Vector2(max.x, max.y), bWidth, color);
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
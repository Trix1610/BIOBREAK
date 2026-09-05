using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    private readonly Dictionary<string, string> roomConnections = new();
    
    public Dictionary<string, Vector2Int> DiscoveredRoomPositions { get; private set; } = new Dictionary<string, Vector2Int>();

    // Список уже зачищенных комнат
    private readonly HashSet<string> clearedRooms = new();
    
    // Новые списки для отслеживания наград
    private readonly HashSet<string> roomsWithPendingReward = new();
    private readonly HashSet<string> roomsRewardCollected = new();

    private readonly string[] rooms =
    {
        "ROOM_00",
        "ROOM_01",
        "ROOM_02",
        "ROOM_03",
        "ROOM_04"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartNewRun();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartNewRun()
    {
        roomConnections.Clear();
        DiscoveredRoomPositions.Clear();
        clearedRooms.Clear();
        roomsWithPendingReward.Clear();
        roomsRewardCollected.Clear();
        
        DiscoveredRoomPositions["ROOM_00"] = new Vector2Int(0, 0);

        GenerateRoute();
    }

    // Методы для проверки и управления наградами
    public bool HasPendingReward(string roomName) => roomsWithPendingReward.Contains(roomName);
    public bool IsRewardCollected(string roomName) => roomsRewardCollected.Contains(roomName);

    public void MarkRewardAsSpawned(string roomName)
    {
        if (!roomsRewardCollected.Contains(roomName))
        {
            roomsWithPendingReward.Add(roomName);
        }
    }

    public void MarkRewardAsCollected(string roomName)
    {
        roomsWithPendingReward.Remove(roomName);
        roomsRewardCollected.Add(roomName);
        Debug.Log($"Награда в комнате {roomName} успешно подобрана!");
    }

    // Срабатывает автоматически при загрузке любой комнаты
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentRoom = scene.name;

        // 1. Автоматически меняем фон главной камеры на темно-серый
        Camera roomCamera = Camera.main;
        if (roomCamera != null)
        {
            roomCamera.clearFlags = CameraClearFlags.SolidColor;
            roomCamera.backgroundColor = new Color(0.15f, 0.15f, 0.15f); // Тёмно-серый цвет
        }

        // 2. Если комната уже зачищена, дополнительно подчищаем оставшиеся объекты
        if (clearedRooms.Contains(currentRoom))
        {
            StartCoroutine(ClearRoomObjectsRoutine());
        }
    }

    private IEnumerator ClearRoomObjectsRoutine()
    {
        yield return null;
        yield return null;

        // Если вдруг какой-то объект проскочил Awake у врага, подчищаем по тегам
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    public void MarkCurrentRoomAsCleared()
    {
        string currentRoom = SceneManager.GetActiveScene().name;
        if (!clearedRooms.Contains(currentRoom))
        {
            clearedRooms.Add(currentRoom);
            Debug.Log($"Комната {currentRoom} зачищена и сохранена в RunManager!");
        }
    }

    public bool IsCurrentRoomCleared()
    {
        string currentRoom = SceneManager.GetActiveScene().name;
        return clearedRooms.Contains(currentRoom);
    }

    private void GenerateRoute()
    {
        List<string> shuffledRooms = new List<string>(rooms);

        Shuffle(shuffledRooms);

        shuffledRooms.Remove("ROOM_00");

        string previousRoom = "ROOM_00";

        foreach (string room in shuffledRooms)
        {
            ConnectRooms(previousRoom, room);
            previousRoom = room;
        }
    }

    private void ConnectRooms(string roomA, string roomB)
    {
        roomConnections[roomA + "|Right"] = roomB;
        roomConnections[roomB + "|Left"] = roomA;
    }

    public string GetDestination(string room, string direction)
    {
        string key = room + "|" + direction;

        if (roomConnections.TryGetValue(key, out string destination))
        {
            return destination;
        }

        return null;
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
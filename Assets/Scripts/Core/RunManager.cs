using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    private readonly Core.RunState runState = new();
    
    public Dictionary<string, Vector2Int> DiscoveredRoomPositions { get; private set; } = new Dictionary<string, Vector2Int>();

    private readonly string[] rooms =
    {
        SceneNames.StartRoom,
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

        ServiceLocator.Register<IRunState>(runState);

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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartNewRun()
    {
        runState.Reset();
        DiscoveredRoomPositions.Clear();
        
        DiscoveredRoomPositions[SceneNames.StartRoom] = new Vector2Int(0, 0);

        GenerateRoute();
    }

    // Методы для проверки и управления наградами
    public bool HasPendingReward(string roomName) => runState.RoomsWithPendingReward.Contains(roomName);
    public bool IsRewardCollected(string roomName) => runState.RoomsRewardCollected.Contains(roomName);

    public void MarkRewardAsSpawned(string roomName)
    {
        if (!runState.RoomsRewardCollected.Contains(roomName))
        {
            runState.RoomsWithPendingReward.Add(roomName);
        }
    }

    public void MarkRewardAsCollected(string roomName)
    {
        runState.RoomsWithPendingReward.Remove(roomName);
        runState.RoomsRewardCollected.Add(roomName);
        Debug.Log($"Награда в комнате {roomName} успешно подобрана!");
    }

    // НОВЫЕ МЕТОДЫ: Сохранение и получение индекса конкретного предмета в комнате
    public void SaveRoomRewardIndex(string roomName, int index)
    {
        runState.RoomRewardIndices[roomName] = index;
    }

    public bool TryGetRoomRewardIndex(string roomName, out int index)
    {
        return runState.RoomRewardIndices.TryGetValue(roomName, out index);
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
        if (runState.ClearedRooms.Contains(currentRoom))
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
        if (!runState.ClearedRooms.Contains(currentRoom))
        {
            runState.ClearedRooms.Add(currentRoom);
            Debug.Log($"Комната {currentRoom} зачищена и сохранена в RunManager!");
        }
    }

    public bool IsCurrentRoomCleared()
    {
        string currentRoom = SceneManager.GetActiveScene().name;
        return runState.ClearedRooms.Contains(currentRoom);
    }

    private void GenerateRoute()
    {
        List<string> shuffledRooms = new List<string>(rooms);

        Shuffle(shuffledRooms);

        shuffledRooms.Remove(SceneNames.StartRoom);

        string previousRoom = SceneNames.StartRoom;

        foreach (string room in shuffledRooms)
        {
            ConnectRooms(previousRoom, room);
            previousRoom = room;
        }
    }

    private void ConnectRooms(string roomA, string roomB)
    {
        runState.RoomConnections[roomA + "|Right"] = roomB;
        runState.RoomConnections[roomB + "|Left"] = roomA;
    }

    public string GetDestination(string room, string direction)
    {
        string key = room + "|" + direction;

        if (runState.RoomConnections.TryGetValue(key, out string destination))
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
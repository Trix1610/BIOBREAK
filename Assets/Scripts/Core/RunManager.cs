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

    [Header("Background Settings")]
    [SerializeField] private Sprite roomBackgroundSprite;

    [Header("Platform Settings")]
    [SerializeField] private Sprite platformSprite;

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

    public void SaveRoomRewardIndex(string roomName, int index)
    {
        runState.RoomRewardIndices[roomName] = index;
    }

    public bool TryGetRoomRewardIndex(string roomName, out int index)
    {
        return runState.RoomRewardIndices.TryGetValue(roomName, out index);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentRoom = scene.name;
        Debug.Log($"[RunManager] Загружена сцена: {currentRoom}");

        SetupRoomBackground();
        SetupPlatforms();

        if (runState.ClearedRooms.Contains(currentRoom))
        {
            StartCoroutine(ClearRoomObjectsRoutine());
        }
    }

    private void SetupRoomBackground()
    {
        Camera roomCamera = Camera.main;
        if (roomCamera != null)
        {
            roomCamera.clearFlags = CameraClearFlags.SolidColor;
            roomCamera.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        }

        if (roomBackgroundSprite == null) return;

        GameObject bgObj = GameObject.Find("GeneratedRoomBackground");
        if (bgObj == null)
        {
            bgObj = new GameObject("GeneratedRoomBackground");
            
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sprite = roomBackgroundSprite;
            sr.sortingOrder = -10; 
            bgObj.transform.position = new Vector3(0f, 0f, 0f);
        }
    }

    private void SetupPlatforms()
    {
        if (platformSprite == null)
        {
            platformSprite = Resources.Load<Sprite>("platform_0");
            if (platformSprite == null)
            {
                Debug.LogError("[RunManager] ОШИБКА: platformSprite не назначен в инспекторе и не найден в Resources!");
                return;
            }
        }

        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        int wallsLayerIndex = LayerMask.NameToLayer("Walls");

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int platformCount = 0;

        foreach (GameObject obj in allObjects)
        {
            bool isGround = (groundLayerIndex != -1 && obj.layer == groundLayerIndex);
            bool isWall = (wallsLayerIndex != -1 && obj.layer == wallsLayerIndex);

            if (isGround || isWall)
            {
                platformCount++;

                Transform visualChild = obj.transform.Find("PlatformVisual");
                GameObject visualObj;

                if (visualChild == null)
                {
                    visualObj = new GameObject("PlatformVisual");
                    visualObj.transform.SetParent(obj.transform);
                }
                else
                {
                    visualObj = visualChild.gameObject;
                }

                visualObj.transform.localPosition = Vector3.zero;
                visualObj.transform.localRotation = Quaternion.identity;

                SpriteRenderer sr = visualObj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = visualObj.AddComponent<SpriteRenderer>();
                }

                sr.sprite = platformSprite;
                sr.sortingOrder = 1;

                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null && sr.sprite != null)
                {
                    visualObj.transform.localPosition = collider.offset;

                    Vector2 spriteSize = sr.sprite.bounds.size;
                    if (spriteSize.x > 0 && spriteSize.y > 0)
                    {
                        float scaleX = collider.size.x / spriteSize.x;
                        float scaleY = collider.size.y / spriteSize.y;

                        visualObj.transform.localScale = new Vector3(
                            scaleX > 0 ? scaleX : 1f, 
                            scaleY > 0 ? scaleY : 1f, 
                            1f
                        );
                    }
                }
                else
                {
                    visualObj.transform.localScale = Vector3.one;
                }
            }
        }

        Debug.Log($"[RunManager] Настроено объектов (Ground/Walls): {platformCount}");
    }

    private IEnumerator ClearRoomObjectsRoutine()
    {
        yield return null;
        yield return null;

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
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    private readonly Dictionary<string, string> roomConnections = new();

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

    public void StartNewRun()
    {
        roomConnections.Clear();

        GenerateRoute();
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

        if (roomConnections.TryGetValue(
                key,
                out string destination))
        {
            return destination;
        }

        Debug.LogError(
            $"RunManager: connection not found: {key}"
        );

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
